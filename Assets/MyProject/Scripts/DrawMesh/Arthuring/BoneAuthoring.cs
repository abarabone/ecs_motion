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

using Abss.Instance;
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

        //public bool UsePhysics;

        public AvatarMask BoneMask;


        public (NativeArray<Entity> bonePrefabs, Entity posturePrefab) Convert
            ( EntityManager em, NativeArray<Entity> streamPrefabs, Entity drawPrefab )
        {
            Debug.Log( this.BoneMask.transformCount );
            foreach( var x in Enumerable.Range( 0, BoneMask.transformCount ) )
                Debug.Log( $"{x} {this.BoneMask.GetTransformActive( x )} {this.BoneMask.GetTransformPath( x ) }" );

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//
            var bindposes = this.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.bindposes;//
            var mtBones = (bindposes, motionClip.IndexMapFbxToMotion).Zip()
                .Where( x => x.y != -1 )
                .OrderBy( x => x.y )
                .Select( x => new float4x4(x.x.GetRow(0), x.x.GetRow(1), x.x.GetRow(2), x.x.GetRow(3)) )
                .Select( x => math.inverse(x) )
                .ToArray();
            //var boneMasks = (Enumerable.Range( 1, bindposes.Length ), motionClip.IndexMapFbxToMotion).Zip()
            //    .Where( x => x.y != -1 )
            //    .OrderBy( x => x.y )
            //    .Select( x => this.BoneMask.GetTransformActive( x.x ) )
            //    .ToArray();
            var boneMasks = new bool[] {
                true, true, true, true, true,
                true, true, true, true, true,
                true, true, true, true, false, false };

            return BonePrefabCreator.CreatePrefabs( em, streamPrefabs, drawPrefab, motionClip, mtBones, boneMasks );
        }
    }


    static public class BonePrefabCreator
    {
        
        static EntityArchetypeCache boneArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( BoneRelationLinkData ),
                typeof( BoneDrawLinkData ),
                typeof( BoneStreamLinkData ),
                typeof( BoneIndexData ),
                typeof( BoneDrawTargetIndexWorkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( Prefab )
            )
        );

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


        static public (NativeArray<Entity> bonePrefabs, Entity posturePrefab) CreatePrefabs(
            EntityManager em,
            NativeArray<Entity> streamPrefabs, Entity drawPrefab,
            MotionClip motionClip, float4x4[] mtBones, bool[] boneMasks
        )
        {
            var postArchetype = postureArchetypeCache.GetOrCreateArchetype( em );
            var boneArchetype = boneArchetypeCache.GetOrCreateArchetype( em );

            var posturePrefab = em.CreateEntity( postArchetype );

            
            var bonePrefabs = createBonePrefabs( em, mtBones.Length, boneArchetype );
            setBoneId( em, bonePrefabs, drawPrefab );
            setStreamLinks( em, bonePrefabs, streamPrefabs, mtBones.Length );
            setDrawLinks( em, bonePrefabs, drawPrefab, mtBones.Length );
            setBoneRelationLinks( em, bonePrefabs, posturePrefab, motionClip, boneMasks );

            foreach( var (bonePrefab, mtBone) in (bonePrefabs, mtBones).Zip() )
            {
                em.SetComponentData( bonePrefab, new Rotation { Value = new quaternion(mtBone) } );
                em.SetComponentData( bonePrefab, new Translation { Value = math.transpose(mtBone).c3.As_float3() } );
            }


            em.SetComponentData( posturePrefab, new PostureLinkData { BoneRelationTop = bonePrefabs[ 0 ] } );
            em.SetComponentData( posturePrefab, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( posturePrefab, new Translation { Value = float3.zero } );

            return (bonePrefabs, posturePrefab);

            
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
                EntityManager em_, NativeArray<Entity> bonePrefabs_, NativeArray<Entity> streamPrefabs_,
                int boneLength
            )
            {
                var qPosStreams = streamPrefabs_
                    .Take( boneLength );
                var qRotStreams = streamPrefabs_
                    .Skip( boneLength )
                    .Take( boneLength );
                var qStreamlinkers =
                    from ent in (qPosStreams, qRotStreams).Zip()
                    select new BoneStreamLinkData
                    {
                        PositionStreamEntity = ent.x,
                        RotationStreamEntity = ent.y,
                    };

                em_.SetComponentData( bonePrefabs_, qStreamlinkers );

                foreach( var bonePrefab in bonePrefabs_ )
                {
                    var linker = em_.GetComponentData<BoneStreamLinkData>( bonePrefab );
                    if( linker.PositionStreamEntity != Entity.Null && linker.RotationStreamEntity != Entity.Null ) continue;
                    em_.RemoveComponent<BoneStreamLinkData>( bonePrefab );
                }
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

            unsafe void setBoneRelationLinks(
                EntityManager em_,
                NativeArray<Entity> bonePrefabs_, Entity posturePrefab_,
                MotionClip motionClip_, bool[] boneMasks_
            )
            {
                var qParentIds = motionClip_.StreamPaths
                    .QueryParentIdList();

                var qEnts = bonePrefabs_
                    .Prepend( posturePrefab_ )
                    .Append( Entity.Null );

                using( var parentIds = qParentIds.ToNativeArray(Allocator.Temp) )
                using( var boneEnts = qEnts.ToNativeArray(Allocator.Temp) )
                {
                    var boneLength = motionClip_.StreamPaths.Length;
                    
                    var qBoneLinker =
                        from i in Enumerable.Range( 0, boneLength )
                        let parentId = parentIds[ i ]
                        let nextId = i + 1
                        select new BoneRelationLinkData
                        {
                            ParentBoneEntity = boneEnts[ parentId + 1 ],// +1 は、ルートの親が -1 なので 0 に正すため
                            NextBoneEntity = boneEnts[ nextId + 1 ],
                        };

                    var q =
                        from x in (qBoneLinker, boneMasks_).Zip()
                        let linker = x.x
                        let isEnable = x.y
                        where isEnable
                        select new BoneRelationLinkData
                        {
                            ParentBoneEntity = linker.ParentBoneEntity,
                            NextBoneEntity = 
                        };

                    // マスクのかかったボーンを、チェインから抜き取る
                    var prev = Entity.Null;
                    foreach( var (ent, i) in bonePrefabs.Select((x, i) => (x, i)) )
                    {
                        prev = ent;
                        if( boneMasks_[ i ] ) continue;

                        var next = em_.GetComponentData<BoneRelationLinkData>( ent ).NextBoneEntity;
                        em_.SetComponentData( ent, new BoneRelationLinkData { } );

                        var linker = em_.GetComponentData<BoneRelationLinkData>( prev );
                        linker.NextBoneEntity = next;
                        em_.SetComponentData( prev, linker );
                    }

                    em_.SetComponentData( bonePrefabs_, qBoneLinker );
                }
            }

        }
        
    }



}