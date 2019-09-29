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

namespace Abss.Motion
{
    
    public class CharacterGroupArthuring : MonoBehaviour
    {
        
        public CharactorResourceUnit[] Resources;


        PrefabArchetypes prefabArchetypes;

        MotionPrefabUnit[] motionPrefabDatas;


        List<Entity> ents = new List<Entity>();

        void Awake()
        {
            //passResourcesToDrawSystem();
            var w = World.Active;
            var em = w.EntityManager;
            
            this.prefabArchetypes = new PrefabArchetypes(em);

            createPrefabs( em );
            var dat = this.motionPrefabDatas[0];
            var ent = em.Instantiate( dat.Prefab );

            var ma = dat.MotionData.CreateAccessor( 0 );
            em.SetComponentData( ent, new MotionInfoData { DataAccessor =  } );
            this.ents.Add( ent );
        }
        private void Update()
        {
            if( !Input.GetMouseButtonDown(0) ) return;
            
            foreach( var x in this.ents ) World.Active.EntityManager.DestroyEntity(x);
        }

        private void OnDisable()
        {
            //this.motionPrefabDatas.Do( x => x.Dispose() );// .Do() が機能してない？？
            foreach( var x in this.motionPrefabDatas )
                x.Dispose();
        }


        void createPrefabs( EntityManager em )
        {
            var qPrefabs =
                from x in this.Resources.Select((res,id)=>(id,res))
                select new MotionPrefabUnit
                {
                    Prefab = createMotionPrefab( em, x.res.MotionClip, this.prefabArchetypes ),
                    MotionData = x.res.MotionClip.ConvertToNativeData(),
                }
                ;
            this.motionPrefabDatas = qPrefabs.ToArray();
        }
        static Entity createMotionPrefab( EntityManager em, MotionClip motionClip, PrefabArchetypes prefabArchetypes )
        {
            // モーションエンティティ生成
            var motionEntity = em.CreateEntity( prefabArchetypes.Motion );

            // ストリームエンティティ生成
            var streamEntities = new NativeArray<Entity>( motionClip.StreamPaths.Length * 2, Allocator.Temp );
            em.CreateEntity( prefabArchetypes.MotionStream, streamEntities );

            // リンク生成
            var linkedEntityGroup = streamEntities
                .Select( streamEntity => new LinkedEntityGroup { Value = streamEntity } )
                .Prepend( new LinkedEntityGroup { Value = motionEntity } )
                .ToNativeArray( Allocator.Temp );

            // バッファに追加
            var mbuf = em.AddBuffer<LinkedEntityGroup>( motionEntity );
            mbuf.AddRange( linkedEntityGroup );

            // 一時領域破棄
            streamEntities.Dispose();
            linkedEntityGroup.Dispose();

            return motionEntity;
        }


        void passResourcesToDrawSystem()
        {

            foreach( var x in this.Resources.Select((res,id)=>(id,res)) )
            {
                
            }
        }
        static MeshRenderingUnit createRenderingUnit( MotionClip motionClip )
        {
            return new MeshRenderingUnit();
        }

        

        class PrefabArchetypes
        {

            public readonly EntityArchetype Motion;
            public readonly EntityArchetype MotionStream;


            public PrefabArchetypes( EntityManager em )
            {
                this.Motion = em.CreateArchetype
                (
                    //typeof(MotionDataData),
                    typeof(MotionInfoData),
                    typeof(MotionInitializeData),
                    typeof(LinkedEntityGroup),
                    typeof(Prefab)
                );
                this.MotionStream = em.CreateArchetype
                (
                    typeof(StreamKeyShiftData),
                    typeof(StreamNearKeysCacheData),
                    typeof(StreamTimeProgressData),
                    typeof(StreamInitialTag),
                    typeof(Prefab)
                );
            }
        }

    }



    public struct MotionPrefabUnit : IDisposable
    {
        public Entity Prefab;
        public MotionDataInNative MotionData;

        public void Dispose() => this.MotionData.Dispose();
    }




    [Serializable]
    public struct CharactorResourceUnit
    {
        public Mesh[] SkinnedMesh;
        public Material Material;
        public MotionClip MotionClip;
    }


    public class MeshRenderingHolder
    {

        public MeshRenderingUnit[] Units { get; private set; }
        

        public MeshRenderingHolder( IEnumerable<CharactorResourceUnit> resources )
        {
            
            this.Units = resources
                .Select( (res,i) =>
                    new MeshRenderingUnit
                    {
                        MeshId = i,
                        Mesh = combineAndConvertMesh( res.SkinnedMesh, res.MotionClip ),
                        Material = res.Material,
                    }
                )
                .ToArray();
            
            return;

            
            Mesh combineAndConvertMesh( Mesh[] meshes, MotionClip motionClip )
            {
                var qCis =
                    from mesh in meshes
                    select new CombineInstance
                    {
                        mesh = mesh
                    }
                    ;

                var dstmesh = new Mesh();

                dstmesh.CombineMeshes( qCis.ToArray(), mergeSubMeshes:true, useMatrices:false );

                return ChMeshConverter.ConvertToChMesh( dstmesh, motionClip );
            }
        }
    }

    public struct MeshRenderingUnit
    {
        public int MeshId;
        public Mesh Mesh;
        public Material Material;
    }
}


