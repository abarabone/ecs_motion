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



        public (NameAndEntity[] bonePrefabs, Entity posturePrefab) Convert
            ( EntityManager em, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefabs, Entity drawPrefab )
        {
            //Debug.Log( this.BoneMask.transformCount );
            //foreach( var x in Enumerable.Range( 0, BoneMask.transformCount ) )
            //    Debug.Log( $"{x} {this.BoneMask.GetTransformActive( x )} {this.BoneMask.GetTransformPath( x ) }" );

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//
            var bindposes = this.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.bindposes;//
            var mtBones = (bindposes, motionClip.IndexMapFbxToMotion).Zip()
                .Where( x => x.y != -1 )
                .OrderBy( x => x.y )
                .Select( x => new float4x4(x.x.GetRow(0), x.x.GetRow(1), x.x.GetRow(2), x.x.GetRow(3)) )
                .Select( x => math.inverse(x) )
                .ToArray();
            var boneMasks = (Enumerable.Range( 1, bindposes.Length ), motionClip.IndexMapFbxToMotion).Zip()
                .Where( x => x.y != -1 )
                .OrderBy( x => x.y )
                .Select( x => this.BoneMask.GetTransformActive( x.x ) )
                .ToArray();
            if( boneMasks.Length == 0 )
                boneMasks = Enumerable.Repeat( true, bindposes.Length ).ToArray();

            return BonePrefabCreator.CreatePrefabs( em, posStreamPrefabs, rotStreamPrefabs, drawPrefab, motionClip, mtBones, boneMasks );
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
            MotionClip motionClip, float4x4[] mtBones, bool[] boneMasks
        )
        {
            var postArchetype = postureArchetypeCache.GetOrCreateArchetype( em );
            var boneArchetype = boneArchetypeCache.GetOrCreateArchetype( em );

            var posturePrefab = em.CreateEntity( postArchetype );

            
            var bonePrefabs = createBonePrefabs( em, mtBones.Length, boneArchetype );
            setBoneId( em, bonePrefabs, drawPrefab );
            setStreamLinks( em, bonePrefabs, streamPrefabs.Select(x=>x.Entity), boneMasks );
            setDrawLinks( em, bonePrefabs, drawPrefab, mtBones.Length );
            setBoneRelationLinks( em, bonePrefabs, posturePrefab, motionClip, boneMasks );
            removeBoneRelationLinks( em, bonePrefabs, boneMasks );

            //foreach( var (bonePrefab, mtBone) in (bonePrefabs, mtBones).Zip() )
            //{
            //    em.SetComponentData( bonePrefab, new Rotation { Value = new quaternion(mtBone) } );
            //    em.SetComponentData( bonePrefab, new Translation { Value = math.transpose(mtBone).c3.As_float3() } );
            //}


            em.SetComponentData( posturePrefab, new PostureLinkData { BoneRelationTop = bonePrefabs[ 0 ] } );
            em.SetComponentData( posturePrefab, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( posturePrefab, new Translation { Value = float3.zero } );

            var qNameAndBone = motionClip.StreamPaths
                .Select( x => System.IO.Path.GetFileName( x ) )
                .Select( ( name, i ) => (name, i: motionClip.IndexMapFbxToMotion[ i ]) )
                .Where( x => x.i != -1 )// ボーン対象外を省く
                .Select( x => new NameAndEntity( x.name, bonePrefabs[ x.i ] ) );
            var namesAndBones = qNameAndBone.ToArray();

            bonePrefabs.Dispose();
            return (namesAndBones, posturePrefab);

            
            NativeArray<Entity> createBonePrefabs
                ( EntityManager em_, int boneLength, EntityArchetype archetype )
            {
                var bonePrefabs_ = new NativeArray<Entity>( boneLength, Allocator.Temp );

                em_.CreateEntity( archetype, bonePrefabs_ );

                return bonePrefabs_;
            }

            void setBoneId( EntityManager em_, NativeArray<Entity> bonePreafabs_, Entity drawPrefab_ )
            {
                var draw = em_.GetComponentData<DrawModelIndexData>( drawPrefab_ );

                em_.SetComponentData( bonePreafabs_,
                    from x in Enumerable.Range( 0, bonePreafabs_.Length )
                    select new BoneIndexData { ModelIndex = draw.ModelIndex, BoneId = x }
                );
            }

            void setStreamLinks(
                EntityManager em_, NativeArray<Entity> bonePrefabs_, IEnumerable<Entity> streamPrefabs_,
                bool[] boneMasks_
            )
            {
                var enableLength = boneMasks_.Where( x => x ).Count();

                var qPosStream = streamPrefabs_
                    .Take( enableLength );
                var qRotStream = streamPrefabs_
                    .Skip( enableLength )
                    .Take( enableLength );
                var qStreamlinkers =
                    from x in (qPosStream, qRotStream).Zip()
                    let posEnt = x.x
                    let rotEnt = x.y
                    //select new BoneStreamLinkData
                    //{
                    //    PositionStreamEntity = posEnt,
                    //    RotationStreamEntity = rotEnt,
                    //};
                select new BoneStreamLinkBlend2Data
                {
                    PositionStream0Entity = posEnt,
                    RotationStream0Entity = rotEnt,
                    PositionStream1Entity = posEnt,
                    RotationStream1Entity = rotEnt,
                    weight0 = 1.0f,
                };

                var qBoneEnt =
                    from x in (bonePrefabs_, boneMasks_).Zip()
                    let ent = x.x
                    let isEnable = x.y
                    where isEnable
                    select ent
                    ;

                em_.AddComponentData( qBoneEnt, qStreamlinkers );
            }

            void setDrawLinks(
                EntityManager em_,
                NativeArray<Entity> bonePrefabs_, Entity drawPrefab_, int boneLength
            )
            {
                var qDrawLinker = Enumerable
                    .Repeat( new BoneDrawLinkData { DrawEntity = drawPrefab_, }, boneLength );

                em_.SetComponentData( bonePrefabs_, qDrawLinker );
            }

            void setBoneRelationLinks(
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
                    .Select( i => sourceEnts[i+1] );
                var qParent =
                    from x in (qParentEnt, boneMasks_).Zip()
                    let ent = x.x
                    let isEnable = x.y
                    where isEnable
                    select ent
                    ;

                var qNext = qCurrent
                    .Append( Entity.Null )
                    .Skip(1);


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

            void removeBoneRelationLinks
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



}