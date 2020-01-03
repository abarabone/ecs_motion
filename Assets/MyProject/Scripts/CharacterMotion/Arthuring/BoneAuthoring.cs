using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

using Abss.Character;
using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    [Serializable]
    public struct MotionTargetUnit
    {
        public MotionAuthoring Motion;
        public float Weight;
    }

    [DisallowMultipleComponent]
    public class BoneAuthoring : MonoBehaviour
    {

        public AvatarMask BoneMask;


        public EnBoneType Mode;
        public enum EnBoneType
        {
            reel_a_chain,
            in_deep_order,
        }


        // ボーン
        // ・ボーンＩＤは、SkinnedMeshRenderer.bones の並び順
        // ・ボーン名が _ で始まるものは除外
        // ・除外したうえでＩＤを 0 から振りなおし
        // ・モーションストリームはボーンに対応するようにソートされている
        // ・ボーンとマスクの並び順は同じだと思われるが、念のためボーン名で取得する

        public (NameAndEntity[] bonePrefabs, Entity posturePrefab) Convert(
            EntityManager em,
            Entity mainMotionPrefab,
            IEnumerable<(StreamEntityUnit[],EnMotionBlendingType)> streamPrefabss,
            Entity drawPrefab
        )
        {


            var smr = this.GetComponentInChildren<SkinnedMeshRenderer>();//
            
            var enabledsAndPaths = queryEnabledsAndPaths_().ToArray();
            var mtBones = queryBoneMatrixes_().ToArray();

            return BonePrefabCreator.CreatePrefabs
                ( em, drawPrefab, mainMotionPrefab, streamPrefabss, mtBones, enabledsAndPaths );



            IEnumerable<float4x4> queryBoneMatrixes_()
            {
                var bindposes = ChMeshConverter.QueryEnabledBindPoses( smr.sharedMesh.bindposes, smr.bones );

                return
                    from x in bindposes
                    let mt = new float4x4( x.GetRow( 0 ), x.GetRow( 1 ), x.GetRow( 2 ), x.GetRow( 3 ) )
                    select math.inverse( mt )
                    ;
            }

            // ここで得られるパスは、ルートがヒエラルキールートになってしまうため、ストリームやマスクのパスとは異なる。
            IEnumerable<(bool isEnabled, string path)> queryEnabledsAndPaths_()
            {
                if( this.BoneMask == null )
                    return queryBonePath(smr).Select( x => (isEnabled: true, x) );

                return
                    from bpath in queryBonePath(smr)
                    join x in
                        from id in Enumerable.Range( 0, this.BoneMask.transformCount )
                        select (isEnabled: this.BoneMask.GetTransformActive(id), path: this.BoneMask.GetTransformPath(id))
                            on System.IO.Path.GetFileName(bpath) equals System.IO.Path.GetFileName(x.path)
                    select (x.isEnabled, bpath)
                    ;

                IEnumerable<string> queryBonePath( SkinnedMeshRenderer smr_ ) =>
                    smr_.bones
                        .Where( x => !x.name.StartsWith( "_" ) )
                        .Select( x => x.gameObject )
                        .MakePath();
            }
        }
    }

    
    static public class BonePrefabCreator
    {

        static EntityArchetypeCache postureArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( PostureNeedTransformTag ),
                typeof( PostureLinkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( Prefab )
            )
        );

        static EntityArchetypeCache boneArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( BoneRelationLinkData ),
                typeof( BoneDrawLinkData ),
                //typeof( BoneStream0LinkData ),// 剛体には必要ないので必要な場合に add するようにした　ブレンドの場合には複数必要だし
                typeof( BoneIndexData ),
                typeof( BoneDrawTargetIndexWorkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( BoneLocalValueData ),// どうしようか
                typeof( Prefab )
            )
        );

        
        static public (NameAndEntity[] bonePrefabs, Entity posturePrefab) CreatePrefabs
        (
            EntityManager em,
            Entity drawPrefab, Entity mainMotionPrefab,
            IEnumerable<(StreamEntityUnit[] streams,EnMotionBlendingType blendType)> streamPrefabss,
            float4x4[] mtBones, (bool isEnabled, string path)[] enabledsAndPaths
        )
        {
            var postArchetype = postureArchetypeCache.GetOrCreateArchetype( em );
            var boneArchetype = boneArchetypeCache.GetOrCreateArchetype( em );

            var qName =
                from x in enabledsAndPaths
                select System.IO.Path.GetFileName( x.path )
                ;
            var qBoneMasks =
                from x in enabledsAndPaths
                select x.isEnabled
                ;

            var posturePrefab = em.CreateEntity( postArchetype );

            var boneNameAndPrefabs = createNameAndBonePrefabs( em, qName, boneArchetype );
            var bonePrefabs = (from x in boneNameAndPrefabs select x.Entity).ToArray();

            em.SetComponentData( bonePrefabs, new BoneDrawLinkData { DrawEntity = drawPrefab } );
            em.setBoneId( bonePrefabs, drawPrefab );
            em.setBoneRelationLinks( posturePrefab, boneNameAndPrefabs, enabledsAndPaths );
            em.removeBoneRelationLinks( bonePrefabs, qBoneMasks );
            em.addBoneStreamLinkData( mainMotionPrefab, boneNameAndPrefabs, streamPrefabss );

            em.SetComponentData( posturePrefab, new PostureLinkData { BoneRelationTop = bonePrefabs[ 0 ] } );
            em.SetComponentData( posturePrefab, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( posturePrefab, new Translation { Value = float3.zero } );

            em.addMotionBendWeightData( mainMotionPrefab, streamPrefabss.Count( x => x.blendType != EnMotionBlendingType.overwrite ) );

            return (boneNameAndPrefabs, posturePrefab);
        }



        static NameAndEntity[] createNameAndBonePrefabs
            ( this EntityManager em_, IEnumerable<string> qName, EntityArchetype archetype )
        {
            using( var bonePrefabs = new NativeArray<Entity>( qName.Count(), Allocator.Temp ) )
            {
                em_.CreateEntity( archetype, bonePrefabs );

                var q =
                    from x in (qName, bonePrefabs).Zip()
                    select new NameAndEntity( x.x, x.y )
                    ;
                return q.ToArray();
            }
        }

        static void setBoneId
            ( this EntityManager em_, IEnumerable<Entity> bonePreafabs_, Entity drawPrefab_ )
        {
            var draw = em_.GetComponentData<DrawIndexOfModelData>( drawPrefab_ );

            em_.SetComponentData( bonePreafabs_,
                from x in Enumerable.Range( 0, bonePreafabs_.Count() )
                select new BoneIndexData { BoneLength = draw.BoneLength, BoneId = x }
            );
        }
        
        static void setBoneRelationLinks(
            this EntityManager em_,
            Entity posturePrefab_, NameAndEntity[] bonePrefabs_,
            (bool isEnabled, string path)[] enabledsAndPaths_
        )
        {
            var qParentEnt =
                from parentName in
                    from src in enabledsAndPaths_
                    where src.isEnabled
                    let parentPath = System.IO.Path.GetDirectoryName( src.path )
                    select System.IO.Path.GetFileName(parentPath)
                join bone in bonePrefabs_
                    on parentName equals bone.Name
                into x
                from bone in x.DefaultIfEmpty(new NameAndEntity("",posturePrefab_))
                select bone.Entity
                ;

            var qNextEnt = (bonePrefabs_, enabledsAndPaths_).Zip( ( x, y ) => (x.Entity, y.isEnabled))
                .Where( x => x.isEnabled )
                .Select( x => x.Entity )
                .Append( Entity.Null )
                .Skip( 1 );

            var qBoneLinker =
                from x in (qParentEnt, qNextEnt).Zip()
                select new BoneRelationLinkData
                {
                    ParentBoneEntity = x.x,
                    NextBoneEntity = x.y,
                };

            em_.SetComponentData( from x in bonePrefabs_ select x.Entity, qBoneLinker );

        }

        // チャンクが別になるから、消さないほうがいい可能性もあり
        static void removeBoneRelationLinks
            ( this EntityManager em_, IEnumerable<Entity> bonePrefabs_, IEnumerable<bool> boneMasks_ )
        {
            var qDisEnables =
                from x in (bonePrefabs_, boneMasks_).Zip()
                let boneEnt = x.x
                let isEnable = x.y
                where !isEnable
                select boneEnt
                ;
            foreach( var x in qDisEnables )
            {
                em_.RemoveComponent<BoneRelationLinkData>( x );
                em_.RemoveComponent<BoneStream0LinkData>( x );
            }

        }


        static void addMotionBendWeightData( this EntityManager em, Entity mainMotionPrefab, int motionLength )
        {
            switch( motionLength )
            {
                case 2: em.AddComponentData( mainMotionPrefab, new MotionBlend2WeightData { WeightNormalized0 = 1.0f } );
                    break;
                //case 3: em.AddComponentData( mainMotionPrefab, new MotionBlend3WeightData { } );
                //    break;
            }
        }

        static void addBoneStreamLinkData(
            this EntityManager em, Entity mainMotionPrefab,
            NameAndEntity[] bonePrefabs,
            IEnumerable<(StreamEntityUnit[] streams, EnMotionBlendingType blendType)> streamPrefabss
        )
        {
            var qStreamWithChannelByName =
                from motion in streamPrefabss
                where motion.blendType != EnMotionBlendingType.overwrite
                from st in motion.streams
                group (channel: motion.blendType, st.Position, st.Rotation, st.Scale) by st.Name
                ;
            var qBoneLinked =
                from bn in bonePrefabs
                join st in qStreamWithChannelByName
                    on bn.Name equals st.Key
                select (bone:bn.Entity, streams:st)
                ;

            var q =
                from bn in qBoneLinked
                from st in bn.streams
                select (bn, st)
                ;
            foreach( var (bn, st) in q )
            {
                switch( st.channel )
                {
                    case EnMotionBlendingType.blendChannel0:
                    {
                        var linker = new BoneStream0LinkData
                        {
                            PositionStreamEntity = st.Position,
                            RotationStreamEntity = st.Rotation,
                        };
                        em.AddComponentData( bn.bone, linker );
                    }
                    break;
                    case EnMotionBlendingType.blendChannel1:
                    {
                        var linker = new BoneStream1LinkData
                        {
                            PositionStreamEntity = st.Position,
                            RotationStreamEntity = st.Rotation,
                        };
                        em.AddComponentData( bn.bone, linker );
                    }
                    break;
                    //case 2:
                    //{
                    //    var linker = new BoneStream2LinkData
                    //    {
                    //        PositionStreamEntity = st.Position,
                    //        RotationStreamEntity = st.Rotation,
                    //    };
                    //    em.AddComponentData( bn.bone, linker );
                    //}
                    //break;
                }
            }
            //foreach( var x in qBoneLinked )
            //{
            //    var ist = x.streams.GetEnumerator();


            //    if( !ist.MoveNext() ) continue;

            //    var st0 = ist.Current;
            //    var linker0 = new BoneStream0LinkData
            //    {
            //        PositionStreamEntity = st0.Position,
            //        RotationStreamEntity = st0.Rotation,
            //    };
            //    em.AddComponentData( x.bone, linker0 );


            //    if( !ist.MoveNext() ) continue;

            //    em.AddComponentData( x.bone, new BoneMotionBlendLinkData { MotionBlendEntity = mainMotionPrefab } );

            //    var st1 = ist.Current;
            //    var linker1 = new BoneStream1LinkData
            //    {
            //        PositionStreamEntity = st1.Position,
            //        RotationStreamEntity = st1.Rotation,
            //    };
            //    em.AddComponentData( x.bone, linker1 );

            //    if( !ist.MoveNext() ) continue;

            //    var st2 = ist.Current;
            //    var linker2 = new BoneStream2LinkData
            //    {
            //        PositionStreamEntity = st2.Position,
            //        RotationStreamEntity = st2.Rotation,
            //    };
            //    em.AddComponentData( x.bone, linker2 );
            //}
        }
    }



}