using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Charactor;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    public class MotionAuthoring : MonoBehaviour
    {

        public MotionClip MotionClip;


        public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) Convert
            ( EntityManager em, MotionPrefabCreator motionCreator )
        {
            var motionClip = this.MotionClip;
            
            return motionCreator.CreatePrefab( em, motionClip );
        }
    }



    public class MotionPrefabCreator
    {

        
        EntityArchetype motionPrefabArchetype;
        EntityArchetype streamPrefabArchetype;



        public MotionPrefabCreator( EntityManager em )
        {
            
            this.motionPrefabArchetype = em.CreateArchetype
            (
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionInitializeData ),
                typeof( Prefab )
            );

            this.streamPrefabArchetype = em.CreateArchetype
            (
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamTimeProgressData ),
                typeof( Prefab )
            );
        }


        public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) CreatePrefab
            ( EntityManager em, MotionClip motionClip )
        {

            var motionBlobData = motionClip.ConvertToBlobData();

            var motionPrefab = createMotionPrefab( em, motionBlobData, this.motionPrefabArchetype );
            var streamPrefabs = createStreamPrefabs( em, motionBlobData, this.streamPrefabArchetype );

            return (motionPrefab, streamPrefabs);


            // モーションエンティティ生成
            Entity createMotionPrefab
                ( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype motionArchetype_ )
            {
                var motionEntity = em_.CreateEntity( motionArchetype_ );
                em_.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData_ } );

                return motionEntity;
            }

            // ストリームエンティティ生成
            NativeArray<Entity> createStreamPrefabs
                ( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype streamArchetype_ )
            {
                var streamLength = motionBlobData_.Value.BoneParents.Length * 2;
                var streamEntities = new NativeArray<Entity>( streamLength, Allocator.Temp );
                em_.CreateEntity( streamArchetype_, streamEntities );

                return streamEntities;
            }
        }

    }


}

