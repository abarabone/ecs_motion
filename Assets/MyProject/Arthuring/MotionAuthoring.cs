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


        public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) Convert( EntityManager em )
        {
            var motionClip = this.MotionClip;
            
            return MotionPrefabCreator.CreatePrefab( em, motionClip );
        }
    }



    static public class MotionPrefabCreator
    {

        
        static EntityArchetypeCache motionArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( MotionInfoData ),
                typeof( MotionClipData ),
                typeof( MotionInitializeData ),
                typeof( Prefab )
            )
        );
        
        static EntityArchetypeCache streamArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamTimeProgressData ),
                typeof( Prefab )
            )
        );


        static public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) CreatePrefab
            ( EntityManager em, MotionClip motionClip )
        {

            var motionArchetype = motionArchetypeCache.GetOrCreateArchetype( em );
            var streamArchetype = streamArchetypeCache.GetOrCreateArchetype( em );
            var motionBlobData = motionClip.ConvertToBlobData();

            var motionPrefab = createMotionPrefab( em, motionBlobData, motionArchetype );
            var streamPrefabs = createStreamPrefabs( em, motionBlobData, streamArchetype );

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

