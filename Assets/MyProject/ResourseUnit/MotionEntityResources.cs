using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Arthuring;
using Abss.Motion;
using Abss.Cs;

namespace Abss.Motion
{
    
    
    public struct MotionPrefabUnit : IDisposable
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
        }

        
        

        EntityArchetype getOrCreateMotionArchetype()
        {
            if( this._motionPrefabArchetype.Valid ) return this._motionPrefabArchetype;
            
            return this._motionPrefabArchetype = this.em.CreateArchetype
            (
                typeof( MotionInfoData ),
                typeof( MotionDataData ),
                typeof( MotionInitializeData ),
                typeof( LinkedEntityGroup ),
                typeof( Prefab )
            );
        }

        EntityArchetype getOrCreateStreamArchetype()
        {
            if( this._streamPrefabArchetype.Valid ) return this._streamPrefabArchetype;

            return this._streamPrefabArchetype = this.em.CreateArchetype
            (
                typeof( StreamKeyShiftData ),
                typeof( StreamNearKeysCacheData ),
                typeof( StreamTimeProgressData ),
                typeof( Prefab )
            );
        }
        
    }



    class MotionPrefabHolder : IDisposable
    {

        public MotionPrefabUnit[] MotionPrefabResources { get; private set; }



        public MotionPrefabHolder( EntityManager em, CharactorResourceUnit[] resources )
        {

            var archetypes = createArchetypes( em );
            
            this.MotionPrefabResources = queryPrefabUnits( resources, archetypes ).ToArray();
            
            return;


            (EntityArchetype motion, EntityArchetype stream) createArchetypes( EntityManager em_ )
            {
                var motionPrefabArchetype = em_.CreateArchetype
                (
                    typeof( MotionInfoData ),
                    typeof( MotionDataData ),
                    typeof( MotionInitializeData ),
                    typeof( LinkedEntityGroup ),
                    typeof( Prefab )
                );
                var streamPrefabArchetype = em_.CreateArchetype
                (
                    typeof( StreamKeyShiftData ),
                    typeof( StreamNearKeysCacheData ),
                    typeof( StreamTimeProgressData ),
                    typeof( Prefab )
                );

                return (motionPrefabArchetype, streamPrefabArchetype);
            }
            
            IEnumerable<MotionPrefabUnit> queryPrefabUnits
                ( CharactorResourceUnit[] resources_, (EntityArchetype motion, EntityArchetype stream) archtypes  )
            {
                return
                    from x in resources_.Select( ( res, id ) => (id, res) )
                    let motionClipData = x.res.MotionClip.ConvertToBlobData()
                    select new MotionPrefabUnit
                    {
                        Prefab = createMotionPrefab( em, motionClipData, archetypes ),
                        MotionBlobData = motionClipData,
                    }
                    ;
            }

            Entity createMotionPrefab
                ( EntityManager em_, BlobAssetReference<MotionBlobData> motionClipData, (EntityArchetype motion, EntityArchetype stream) archtypes )
            {
                // モーションエンティティ生成
                var motionEntity = em_.CreateEntity( archetypes.motion );
                em_.SetComponentData( motionEntity,
                    new MotionDataData
                    {
                        ClipData = motionClipData
                    }
                );

                // ストリームエンティティ生成
                var streamEntities = new NativeArray<Entity>( motionClipData.Value.BoneParents.Length * 2, Allocator.Temp );
                em_.CreateEntity( archetypes.stream, streamEntities );

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

        public void Dispose()
        {
            //this.motionPrefabDatas.Do( x => x.Dispose() );// .Do() が機能しない？？
            foreach( var x in this.MotionPrefabResources )
                x.Dispose();
        }
            
    }

    


            
}