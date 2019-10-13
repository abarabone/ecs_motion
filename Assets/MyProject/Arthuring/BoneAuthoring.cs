using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Charactor;
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


        public NativeArray<Entity> Convert(
            EntityManager em, BonePrefabCreator boneCreator,
            Entity motionPrefab, NativeArray<Entity> streamPrefabs, Entity drawPrefab
        )
        {

            return boneCreator.CreatePrefabs( em, motionPrefab, streamPrefabs, drawPrefab );

        }
    }


    public class BonePrefabCreator
    {

        EntityArchetype BonePrefabArchetype;


        public BonePrefabCreator( EntityManager em )
        {

            this.BonePrefabArchetype = em.CreateArchetype
            (
                typeof( BoneDrawTargetIndexData ),
                typeof( BoneDrawLinkData ),
                typeof( BoneTransformLinkData ),
                typeof( BoneStreamLinkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( Prefab )
            );

        }


        public NativeArray<Entity> CreatePrefabs
            ( EntityManager em, Entity motionPrefab, NativeArray<Entity> streamPrefabs, Entity drawPrefab )
        {

            ref var motionBlobData = ref getMotionBlobData( em, motionPrefab );

            var bonePrefabs = createBonePrefabs( em, motionPrefab, ref motionBlobData );
            setStreamLinks( em, bonePrefabs, streamPrefabs, ref motionBlobData );
            setDrawLinks( em, bonePrefabs, drawPrefab, ref motionBlobData );
            setBoneRelationLinks( em, bonePrefabs, ref motionBlobData );

            return bonePrefabs;


            ref MotionBlobData getMotionBlobData( EntityManager em_, Entity motionPrefab_ )
            {
                var motionClipData = em_.GetComponentData<MotionClipData>( motionPrefab_ );
                return ref motionClipData.ClipData.Value;
            }

            NativeArray<Entity> createBonePrefabs
                ( EntityManager em_, Entity motionPrefab_, ref MotionBlobData motionBlobData_ )
            {
                var boneLength = motionBlobData_.BoneParents.Length;

                var bonePrefabs_ = new NativeArray<Entity>( boneLength, Allocator.Temp );
                em_.CreateEntity( this.BonePrefabArchetype, bonePrefabs_ );

                return bonePrefabs_;
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

            void setDrawLinks
                ( EntityManager em_, NativeArray<Entity> bonePrefabs_, Entity drawPrefab_, ref MotionBlobData motionBlobData_ )
            {
                var boneLength = motionBlobData_.BoneParents.Length;

                var qDrawLinker = Enumerable
                    .Repeat( new BoneDrawLinkData { DrawEntity = drawPrefab_ }, boneLength );

                em_.SetComponentData( bonePrefabs_, qDrawLinker );
            }

            unsafe void setBoneRelationLinks
                ( EntityManager em_, NativeArray<Entity> bonePrefabs_, ref MotionBlobData motionBlobData_ )
            {
                var pBoneParents = (int*)motionBlobData_.BoneParents.GetUnsafePtr();
                var boneLength = motionBlobData_.BoneParents.Length;

                ref var boneParents = ref motionBlobData_.BoneParents;
                

                var qBoneLinker =
                    from x in bonePrefabs_.Select( ( ent, i ) => (ent, i) )
                    let parentId = pBoneParents[ x.i ]
                    select new BoneTransformLinkData
                    {
                        ParentBoneEntity = bonePrefabs_[ parentId ],
                        NextEntity = x.i == boneLength - 1 ? Entity.Null : bonePrefabs_[ x.i + 1 ],
                    };

                em_.SetComponentData( bonePrefabs_, qBoneLinker );
            }

        }
        
    }

    static public class aaa
    {
        static public unsafe T[] ToArray<T>( ref this BlobArray<T> blobArray )
            where T : struct
        {
            var dstArr = new T[ blobArray.Length ];

            

            return dstArr;
        }
    }
}