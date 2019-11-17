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

    public class BoneAuthoring : MonoBehaviour, CharacterAuthoring.IBoneConverter
    {

        public MotionTargetUnit[] Motions;

        public AvatarMask BoneMask;


        public EnBoneType Mode;
        public enum EnBoneType
        {
            reel_a_chain,
            in_deep_order,
        }


        // ボーン
        // ・ボーンＩＤは、SkinnedMeshRenderer.bones の並び順
        // ・ボーン名が _ で始まるものは除外済み
        // ・除外したうえでＩＤを 0 から振りなおされている
        // ・モーションストリームはボーンに対応するようにソートされている
        // ・ボーンとマスクの並び順は同じだと思われる

        public (NameAndEntity[] bonePrefabs, Entity posturePrefab) Convert
            ( EntityManager em, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefabs, Entity drawPrefab )
        {

            var smr = this.GetComponentInChildren<SkinnedMeshRenderer>();//
            
            var enabledsAndPaths = queryEnabledsAndPaths_().ToArray();
            var mtBones = queryBoneMatrixes_().ToArray();

            return BonePrefabCreator.CreatePrefabs
                ( em, drawPrefab, posStreamPrefabs, rotStreamPrefabs, mtBones, enabledsAndPaths );


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
                //typeof( BoneStreamLinkData ),// 剛体には必要ないので必要な場合に add するようにした
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
            Entity drawPrefab, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefabs,
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
            em.addStreamLinks( boneNameAndPrefabs, posStreamPrefabs, rotStreamPrefabs );

            em.SetComponentData( posturePrefab, new PostureLinkData { BoneRelationTop = bonePrefabs[ 0 ] } );
            em.SetComponentData( posturePrefab, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( posturePrefab, new Translation { Value = float3.zero } );

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
            var draw = em_.GetComponentData<DrawModelIndexData>( drawPrefab_ );

            em_.SetComponentData( bonePreafabs_,
                from x in Enumerable.Range( 0, bonePreafabs_.Count() )
                select new BoneIndexData { ModelIndex = draw.ModelIndex, BoneId = x }
            );
        }

        static void addStreamLinks(
            this EntityManager em_, NameAndEntity[] bonePrefabs_,
            NameAndEntity[] posStreamPrefabs_, NameAndEntity[] rotStreamPrefabs_
        )
        {
            var qBoneAndStream =
                from bone in bonePrefabs_
                join pos in posStreamPrefabs_
                    on bone.Name equals pos.Name
                join rot in rotStreamPrefabs_
                    on bone.Name equals rot.Name
                select (bone, pos, rot)
                ;
            foreach( var (bone, pos, rot) in qBoneAndStream )
            {
                em_.AddComponentData( bone.Entity,
                    new BoneStreamLinkData
                    {
                        PositionStreamEntity = pos.Entity,
                        RotationStreamEntity = rot.Entity,
                    }
                );
            }
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
                em_.RemoveComponent<BoneStreamLinkData>( x );
            }

        }
    }



}