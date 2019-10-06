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

    class MotionAuthoring : PrefabSettingsAuthoring.IConvertToPrefab
    {

        public MotionClip MotionClip;


        public override PrefabSettingsAuthoring.IPrefabResourceUnit Convert
            ( PrefabSettingsAuthoring charactorAuthor )
        {

            var motionClip = this.MotionClip;


            return charactorAuthor.MotionPrefabCreator.CreatePrefabResourceUnit( motionClip );

        }
    }



    public class MotionPrefabUnit : PrefabSettingsAuthoring.IPrefabResourceUnit
    {
        public Entity Prefab;
        public BlobAssetReference<MotionBlobData> MotionBlobData;

        public void Dispose() => this.MotionBlobData.Dispose();
    }
    

    public class MotionPrefabCreator
    {


        EntityManager em;

        EntityArchetype _motionPrefabArchetype;
        EntityArchetype _streamPrefabArchetype;



        public MotionPrefabCreator( EntityManager entityManager )
        {

            this.em = entityManager;


            this._motionPrefabArchetype = this.em.CreateArchetype
                (
                    typeof( MotionInfoData ),
                    typeof( MotionDataData ),
                    typeof( MotionInitializeData ),
                    typeof( LinkedEntityGroup ),
                    typeof( Prefab )
                );

            this._streamPrefabArchetype = this.em.CreateArchetype
            (
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamTimeProgressData ),
                typeof( Prefab )
            );
        }


        public MotionPrefabUnit CreatePrefabResourceUnit( MotionClip motionClip )
        {

            var motionArchetype = this._motionPrefabArchetype;
            var streamArchetype = this._streamPrefabArchetype;
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

