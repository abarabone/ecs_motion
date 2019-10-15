using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;

using Abss.Object;
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

    public class BoneAuthoring : MonoBehaviour
    {

        public MotionTargetUnit[] Motions;

        public bool UsePhysics;


        public (NativeArray<Entity> bonePrefabs, Entity posturePrefab) Convert
            ( EntityManager em, Entity motionPrefab, NativeArray<Entity> streamPrefabs, Entity drawPrefab )
        {

            return BonePrefabCreator.CreatePrefabs( em, motionPrefab, streamPrefabs, drawPrefab );
        }
    }


    static public class BonePrefabCreator
    {
        
        static EntityArchetypeCache boneArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( BoneDrawLinkData ),
                typeof( BoneRelationLinkData ),
                typeof( BoneStreamLinkData ),
                typeof( BoneIdData ),
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
                typeof( PostureLinkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( PostureNeedTransformTag ),
                typeof( Prefab )
            )
        );


        static public (NativeArray<Entity> bonePrefabs, Entity posturePrefab) CreatePrefabs
            ( EntityManager em, Entity motionPrefab, NativeArray<Entity> streamPrefabs, Entity drawPrefab )
        {
            var postArchetype = postureArchetypeCache.GetOrCreateArchetype( em );
            var boneArchetype = boneArchetypeCache.GetOrCreateArchetype( em );

            var posturePrefab = em.CreateEntity( postArchetype );


            ref var motionBlobData = ref getMotionBlobData( em, motionPrefab );

            var bonePrefabs = createBonePrefabs( em, motionPrefab, ref motionBlobData, boneArchetype );
            setBoneId( em, bonePrefabs );
            setStreamLinks( em, bonePrefabs, streamPrefabs, ref motionBlobData );
            setDrawLinks( em, bonePrefabs, drawPrefab, ref motionBlobData );
            setBoneRelationLinks( em, bonePrefabs, posturePrefab, ref motionBlobData );


            em.SetComponentData( posturePrefab, new PostureLinkData { BoneRelationTop = bonePrefabs[ 0 ] } );

            return (bonePrefabs, posturePrefab);


            ref MotionBlobData getMotionBlobData( EntityManager em_, Entity motionPrefab_ )
            {
                var motionClipData = em_.GetComponentData<MotionClipData>( motionPrefab_ );
                return ref motionClipData.ClipData.Value;
            }

            NativeArray<Entity> createBonePrefabs
                ( EntityManager em_, Entity motionPrefab_, ref MotionBlobData motionBlobData_, EntityArchetype archetype )
            {
                var boneLength = motionBlobData_.BoneParents.Length;
                var bonePrefabs_ = new NativeArray<Entity>( boneLength, Allocator.Temp );

                em_.CreateEntity( archetype, bonePrefabs_ );

                return bonePrefabs_;
            }

            void setBoneId( EntityManager em_, NativeArray<Entity> bonePreafabs_ )
            {
                em_.SetComponentData( bonePreafabs_,
                    from x in Enumerable.Range( 0, bonePreafabs_.Length )
                    select new BoneIdData { BoneId = x }
                );
            }

            void setStreamLinks(
                EntityManager em_, NativeArray<Entity> bonePrefabs_, NativeArray<Entity> streamPrefabs_,
                ref MotionBlobData motionBlobData_
            )
            {
                var boneLength = motionBlobData_.BoneParents.Length;

                var qPosStreams = streamPrefabs_
                    .Take( boneLength );
                var qRotStreams = streamPrefabs_
                    .Skip( boneLength )
                    .Take( boneLength );
                var qStreamlinkers =
                    from ents in (qPosStreams, qRotStreams).Zip()
                    select new BoneStreamLinkData
                    {
                        PositionStreamEntity = ents.x,
                        RotationStreamEntity = ents.y,
                    };

                em_.SetComponentData( bonePrefabs_, qStreamlinkers );
            }

            void setDrawLinks(
                EntityManager em_,
                NativeArray<Entity> bonePrefabs_, Entity drawPrefab_, ref MotionBlobData motionBlobData_
            )
            {
                var boneLength = motionBlobData_.BoneParents.Length;

                var qDrawLinker = Enumerable
                    .Repeat( new BoneDrawLinkData { DrawEntity = drawPrefab_ }, boneLength );

                em_.SetComponentData( bonePrefabs_, qDrawLinker );
            }

            unsafe void setBoneRelationLinks
                ( EntityManager em_, NativeArray<Entity> bonePrefabs_, Entity posturePrefab_, ref MotionBlobData motionBlobData_ )
            {
                var boneLength = motionBlobData_.BoneParents.Length;
                var pBoneParents = (int*)motionBlobData_.BoneParents.GetUnsafePtr();
                var bones = bonePrefabs_
                    .Prepend( posturePrefab_ )
                    .Append( Entity.Null )
                    .ToArray();
                
                var qBoneLinker =
                    from i in Enumerable.Range(0, boneLength)
                    let parentId = pBoneParents[ i ]
                    let nextId = i + 1
                    select new BoneRelationLinkData
                    {
                        ParentBoneEntity = bones[ parentId + 1 ],// +1 は、ルートの親が -1 なので 0 に正すため
                        NextEntity = bones[ nextId + 1 ],
                    };

                em_.SetComponentData( bonePrefabs_, qBoneLinker );
            }


            //Entity createPosture( EntityManager em_, NativeArray<Entity> bonePrefabs_, EntityArchetype archetype )
            //{
            //    var prefab = em_.CreateEntity( archetype );
            //    em_.SetComponentData( prefab, new PostureLinkData { BoneRelationTop = bonePrefabs_[ 0 ] } );

            //    return prefab;
            //}
        }
        
    }



}