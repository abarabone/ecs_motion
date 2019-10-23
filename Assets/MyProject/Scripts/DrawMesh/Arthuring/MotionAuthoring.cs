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
                typeof( MotionStreamLinkData ),
                typeof( MotionInitializeTag ),
                typeof( Prefab )
            )
        );
        
        static EntityArchetypeCache streamArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( StreamRelationData ),
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamTimeProgressData ),
                typeof( StreamInterpolatedData ),
                typeof( Prefab )
            )
        );


        static public (Entity motionPrefab, NativeArray<Entity> streamPrefabs) CreatePrefab
            ( EntityManager em, MotionClip motionClip )
        {

            var motionArchetype = motionArchetypeCache.GetOrCreateArchetype( em );
            var streamArchetype = streamArchetypeCache.GetOrCreateArchetype( em );

            var motionPrefab = createMotionPrefab( em, motionClip, motionArchetype );
            using( var posStreamPrefabs = createStreamOfSectionPrefabs( em, motionClip, streamArchetype ) )
            using( var rotStreamPrefabs = createStreamOfSectionPrefabs( em, motionClip, streamArchetype ) )
            {

                em.SetComponentData( motionPrefab,
                    new MotionStreamLinkData
                    {
                        PositionStreamTop = posStreamPrefabs[0],
                        RotationStreamTop = rotStreamPrefabs[0],
                    }
                );

                var streamPrefabs = (posStreamPrefabs, rotStreamPrefabs).Concat( Allocator.Temp );
                return (motionPrefab, streamPrefabs);
            }


            // モーションエンティティ生成
            Entity createMotionPrefab
                //( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype motionArchetype_ )
                (EntityManager em_, MotionClip motionClip_, EntityArchetype motionArchetype_)
            {
                var motionBlobData = motionClip.ConvertToBlobData();

                var motionEntity = em_.CreateEntity( motionArchetype_ );
                em_.SetComponentData( motionEntity, new MotionClipData { ClipData = motionBlobData } );
                em_.SetComponentData( motionEntity, new MotionInfoData { MotionIndex = 16 } );

                return motionEntity;
            }

            // ストリームエンティティ生成
            NativeArray<Entity> createStreamOfSectionPrefabs
            //( EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_, EntityArchetype streamArchetype_ )
            ( EntityManager em_, MotionClip motionClip_, EntityArchetype streamArchetype_ )
            {
                var streamLength = motionClip.StreamPaths.Length;
                
                var streamEntities = new NativeArray<Entity>( streamLength, Allocator.Temp );
                em_.CreateEntity( streamArchetype_, streamEntities );

                var qNextLinker = 
                    from ent in streamEntities.Skip( 1 ).Append( Entity.Null )
                    select new StreamRelationData
                    {
                        NextStreamEntity = ent,
                    };
                em_.SetComponentData( streamEntities, qNextLinker );

                return streamEntities;
            }
        }

    }

    // コンポーネントデータを生成して返すだけにして（配列だとしても）、
    // エンティティにくっつけるのとは別にしたほうがいいかも
}

