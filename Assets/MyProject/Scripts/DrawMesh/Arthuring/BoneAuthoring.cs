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
            //Debug.Log( this.BoneMask.transformCount );
            //foreach( var x in Enumerable.Range( 0, BoneMask.transformCount ) )
            //    Debug.Log( $"{x} {this.BoneMask.GetTransformActive( x )} {this.BoneMask.GetTransformPath( x ) }" );

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//
            var smr = this.GetComponentInChildren<SkinnedMeshRenderer>();//
            var bindposes = ChMeshConverter.QueryEnabledBindPoses( smr.sharedMesh.bindposes, smr.bones );//

            var mtBones = bindposes
                .Select( x => new float4x4(x.GetRow(0), x.GetRow(1), x.GetRow(2), x.GetRow(3)) )
                .Select( x => math.inverse(x) )
                .ToArray();

            var enabledsAndPaths = queryEnabledsAndPaths().ToArray();

            return BonePrefabCreator.CreatePrefabs
                ( em, drawPrefab, posStreamPrefabs, rotStreamPrefabs, motionClip, mtBones, enabledsAndPaths );


            IEnumerable<(bool isEnabled, string path)> queryEnabledsAndPaths()
            {
                if( this.BoneMask == null )
                    return smr.bones.Select( x => x.gameObject ).MakePath().Select( x => (isEnabled: true, x) );

                return
                    from id in Enumerable.Range( 0, this.BoneMask.transformCount )
                    select (isEnabled: this.BoneMask.GetTransformActive(id), path: this.BoneMask.GetTransformPath(id))
                    ;
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
            MotionClip motionClip, float4x4[] mtBones, (bool isEnabled, string path)[] enabledsAndPaths
        )
        {
            var postArchetype = postureArchetypeCache.GetOrCreateArchetype( em );
            var boneArchetype = boneArchetypeCache.GetOrCreateArchetype( em );

            var posturePrefab = em.CreateEntity( postArchetype );

            
            var bonePrefabs = createBonePrefabs( em, mtBones.Length, boneArchetype );
            var qNameAndBone =
                from x in (enabledsAndPaths, bonePrefabs).Zip()
                where x.x.isEnabled
                let name = System.IO.Path.GetFileName(x.x.path)
                let ent = x.y
                select new NameAndEntity( name, ent );
            var namesAndBones = qNameAndBone.ToArray();

            setBoneId( em, bonePrefabs, drawPrefab );
            setDrawLinks( em, bonePrefabs, drawPrefab, mtBones.Length );
            setBoneRelationLinks( em, bonePrefabs, posturePrefab, motionClip, boneMasks );
            removeBoneRelationLinks( em, bonePrefabs, boneMasks );

            //foreach( var (bonePrefab, mtBone) in (bonePrefabs, mtBones).Zip() )
            //{
            //    em.SetComponentData( bonePrefab, new Rotation { Value = new quaternion(mtBone) } );
            //    em.SetComponentData( bonePrefab, new Translation { Value = math.transpose(mtBone).c3.As_float3() } );
            //}

            addStreamLinks( em, namesAndBones, posStreamPrefabs, rotStreamPrefabs );

            em.SetComponentData( posturePrefab, new PostureLinkData { BoneRelationTop = bonePrefabs[ 0 ] } );
            em.SetComponentData( posturePrefab, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( posturePrefab, new Translation { Value = float3.zero } );

            bonePrefabs.Dispose();
            return (namesAndBones, posturePrefab);

            
        }



        static NativeArray<Entity> createBonePrefabs
            ( EntityManager em_, int boneLength, EntityArchetype archetype )
        {
            var bonePrefabs_ = new NativeArray<Entity>( boneLength, Allocator.Temp );

            em_.CreateEntity( archetype, bonePrefabs_ );

            return bonePrefabs_;
        }

        static void setBoneId( EntityManager em_, NativeArray<Entity> bonePreafabs_, Entity drawPrefab_ )
        {
            var draw = em_.GetComponentData<DrawModelIndexData>( drawPrefab_ );

            em_.SetComponentData( bonePreafabs_,
                from x in Enumerable.Range( 0, bonePreafabs_.Length )
                select new BoneIndexData { ModelIndex = draw.ModelIndex, BoneId = x }
            );
        }

        static void addStreamLinks(
            EntityManager em_, NameAndEntity[] bonePrefabs_,
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

        static void setDrawLinks(
            EntityManager em_,
            NativeArray<Entity> bonePrefabs_, Entity drawPrefab_, int boneLength
        )
        {
            var qDrawLinker =
                from x in bonePrefabs_
                select new BoneDrawLinkData { DrawEntity = drawPrefab_, }
                ;
            em_.SetComponentData( bonePrefabs_, qDrawLinker );
        }

        static void setBoneRelationLinks(
            EntityManager em_,
            NativeArray<Entity> bonePrefabs_, Entity posturePrefab_,
            MotionClip motionClip_, bool[] boneMasks_
        )
        {

            var qCurrent =
                from x in (bonePrefabs_, boneMasks_).Zip()
                let ent = x.x
                let isEnable = x.y
                where isEnable
                select ent
                ;

            var sourceEnts = bonePrefabs_
                .Prepend( posturePrefab_ )
                .ToArray();
            var qParentEnt = motionClip_.StreamPaths
                .QueryParentIdList()
                .Select( i => sourceEnts[ i + 1 ] );
            var qParent =
                from x in (qParentEnt, boneMasks_).Zip()
                let ent = x.x
                let isEnable = x.y
                where isEnable
                select ent
                ;

            var qNext = qCurrent
                .Append( Entity.Null )
                .Skip( 1 );


            var qBoneLinker =
                from x in (qParent, qNext).Zip()
                let parent = x.x
                let next = x.y
                select new BoneRelationLinkData
                {
                    ParentBoneEntity = parent,
                    NextBoneEntity = next,
                };

            em_.SetComponentData( qCurrent, qBoneLinker );

        }

        // チャンクが別になるから、消さないほうがいい可能性もあり
        static void removeBoneRelationLinks
            ( EntityManager em_, NativeArray<Entity> bonePrefabs_, bool[] boneMasks_ )
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