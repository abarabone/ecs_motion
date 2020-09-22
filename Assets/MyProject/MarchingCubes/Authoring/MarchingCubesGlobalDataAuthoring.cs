using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes.Authoring
{
    using Abarabone.Draw;
    using Abarabone.Model;

    public class MarchingCubesGlobalDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public int MaxCubeInstances;
        public int MaxDrawGridLength;

        public MarchingCubeAsset MarchingCubesAsset;
        public Material SrcMaterial;
        public ComputeShader GridCubeIdSetShader;
        public int maxGridLengthInShader;

        public int BlankFreeStockCapacity;
        public int SolidFreeStockCapacity;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {


            createGlobalDataEntity_(conversionSystem, this.gameObject);

            setGlobalData_(conversionSystem, this.gameObject);

            setResources_(conversionSystem, this.gameObject);//, this.SrcMaterial, this.GridCubeIdSetShader, this.MarchingCubesAsset, this.maxGridLengthInShader);

            return;


            unsafe void createGlobalDataEntity_( GameObjectConversionSystem gcs_, GameObject global_ )
            {
                var em = gcs_.DstEntityManager;


                var ent = gcs_.GetPrimaryEntity(global_);
                
                var types = new ComponentTypes
                (new ComponentType[] {
                    typeof(Resource.Initialize),
                    typeof(CubeGridGlobal.FreeGridStockData),
                    typeof(CubeGridGlobal.DefualtGridData),
                    typeof(CubeGridGlobal.InfoData),
                    typeof(Resource.DrawResourceData),
                    typeof(Resource.DrawBufferData),
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag)
                });

                em.AddComponents(ent, types);
            }


            unsafe void setGlobalData_(GameObjectConversionSystem gcs_, GameObject global_)
            {
                var em = gcs_.DstEntityManager;


                var ent = gcs_.GetPrimaryEntity(global_);

                var defaultBuffers = em.GetBuffer<CubeGridGlobal.DefualtGridData>(ent);
                var blank = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Blank);
                var solid = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Solid);
                defaultBuffers.Add(new CubeGridGlobal.DefualtGridData { DefaultGrid = blank });
                defaultBuffers.Add(new CubeGridGlobal.DefualtGridData { DefaultGrid = solid });


                var freeStockBuffers = em.GetBuffer<CubeGridGlobal.FreeGridStockData>(ent);
                freeStockBuffers.Add(
                    new CubeGridGlobal.FreeGridStockData
                    {
                        FreeGridStocks = new UnsafeList<UIntPtr>(this.BlankFreeStockCapacity, Allocator.Persistent),
                    }
                );
                freeStockBuffers.Add(
                    new CubeGridGlobal.FreeGridStockData
                    {
                        FreeGridStocks = new UnsafeList<UIntPtr>(this.SolidFreeStockCapacity, Allocator.Persistent),
                    }
                );

                em.SetComponentData(ent,
                    new CubeGridGlobal.InfoData
                    {
                        MaxCubeInstanceLength = this.MaxCubeInstances,
                        MaxDrawGridLength = this.MaxDrawGridLength,
                    }
                );
            }


            void setResources_
                (
                    GameObjectConversionSystem gcs_, GameObject global_//,
                    //Material srcMat_, ComputeShader computeShader_,
                    //MarchingCubeAsset asset_, int maxGridLengthInShader_
                )
            {
                var em = gcs_.DstEntityManager;


                var ent = gcs_.GetPrimaryEntity(global_);

                em.SetComponentData(ent,
                    new Resource.DrawResourceData
                    {
                        CubeMaterial = new Material(this.SrcMaterial),//srcMat_),
                        InstatnceMesh = Resource.CreateMesh(),
                        GridCubeIdSetShader = this.GridCubeIdSetShader,//computeShader_,
                    }
                );

                em.SetComponentData(ent,
                    new Resource.Initialize
                    {
                        Asset = this.MarchingCubesAsset,//asset_,
                        MaxGridLengthInShader = this.maxGridLengthInShader,//maxGridLengthInShader_,
                    }
                );
            }


        }

    }
}
