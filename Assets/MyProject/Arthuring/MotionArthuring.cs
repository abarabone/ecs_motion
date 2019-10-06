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

namespace Abss.Arthuring
{
    class MotionArthuring : MonoBehaviour, PrefabSettingsAuthoring.IConvertToPrefab
    {

        public MotionClip MotionClip;


        public Entity Convert( PrefabSettingsAuthoring author )
        {
            var MotionPrefabCreator()
        }


        
        public MotionPrefabUnit CreatePrefabResourceUnit( MotionClip motionClip )
        {

            var motionArchetype = getOrCreateMotionArchetype();
            var streamArchetype = getOrCreateStreamArchetype();
            var motionBlobData = motionClip.ConvertToBlobData();
            
            var prefab = createMotionPrefab( this.em, motionBlobData, motionArchetype, streamArchetype );

            return new MotionPrefabUnit
            {
                Prefab = prefab,
                MotionBlobData = motionBlobData,
            }
            ;


            Entity createMotionPrefab
            (
                EntityManager em_, BlobAssetReference<MotionBlobData> motionBlobData_,
                EntityArchetype motionArchetype_, EntityArchetype streamArchetype_
            )
            {
                // モーションエンティティ生成
                var motionEntity = em_.CreateEntity( motionArchetype_ );
                em_.SetComponentData( motionEntity,
                    new MotionDataData
                    {
                        ClipData = motionBlobData_
                    }
                );

                // ストリームエンティティ生成
                var streamEntities = new NativeArray<Entity>( motionBlobData_.Value.BoneParents.Length * 2, Allocator.Temp );
                em_.CreateEntity( streamArchetype_, streamEntities );

                // リンク生成
                var linkedEntityGroup = streamEntities
                    .Select( streamEntity => new LinkedEntityGroup { Value = streamEntity } )
                    .Prepend( new LinkedEntityGroup { Value = motionEntity } )
                    .ToNativeArray( Allocator.Temp );

                // バッファに追加
                var mbuf = em_.AddBuffer<LinkedEntityGroup>( motionEntity );
                mbuf.AddRange( linkedEntityGroup );

                // 一時領域破棄
                streamEntities.Dispose();
                linkedEntityGroup.Dispose();

                return motionEntity;
            }
        }
        
    }
}

