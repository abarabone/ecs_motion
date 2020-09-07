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



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {


            createGlobalDataEntity_(conversionSystem, this.gameObject);

            setGlobalData_
                (conversionSystem, this.gameObject, this.MaxCubeInstances, this.MaxDrawGridLength);

            setResources_
                (conversionSystem, this.gameObject, this.SrcMaterial, this.GridCubeIdSetShader, this.MarchingCubesAsset, this.maxGridLengthInShader);

            return;


            //unsafe void setGlobalData_(GameObjectConversionSystem gcs_, GameObject global_)
            //{
            //    var em = gcs_.DstEntityManager;

            //    var ent = gcs_.GetPrimaryEntity(global_);
            //    var types = new ComponentTypes
            //    (
            //        typeof(CubeGridGlobal.BufferData),
            //        typeof(CubeGridGlobal.DefualtGridBlankData),
            //        typeof(CubeGridGlobal.DefualtGridSolidData),
            //        typeof(CubeGridGlobal.InfoData),
            //        typeof(ModelPrefabNoNeedLinkedEntityGroupTag)
            //    );
            //    em.AddComponents(ent, types);


            //    // 本当は、system で初期化したいが、OnCreate() や OnRunning() では entity を取得できないようだ…
            //    var buffer = new UnsafeList<UIntPtr>(this.MaxCubeInstances, Allocator.Persistent);
            //    var solid = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Solid);
            //    var blank = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Blank);
            //    buffer.Add((UIntPtr)solid.pUnits);
            //    buffer.Add((UIntPtr)blank.pUnits);


            //    em.SetComponentData(ent,
            //        new CubeGridGlobal.BufferData
            //        {
            //            CubeBuffers = buffer,
            //        }
            //    );
            //    em.SetComponentData(ent,
            //        new CubeGridGlobal.DefualtGridSolidData
            //        {
            //            DefaultGrid = solid,
            //        }
            //    );
            //    em.SetComponentData(ent,
            //        new CubeGridGlobal.DefualtGridBlankData
            //        {
            //            DefaultGrid = blank,
            //        }
            //    );
            //    em.SetComponentData(ent,
            //        new CubeGridGlobal.InfoData
            //        {
            //            MaxCubeInstances = this.MaxCubeInstances,
            //            MaxDrawGridLength = this.MaxDrawGridLength,
            //        }
            //    );
            //}
            unsafe void createGlobalDataEntity_( GameObjectConversionSystem gcs_, GameObject global_ )
            {
                var em = gcs_.DstEntityManager;


                var ent = gcs_.GetPrimaryEntity(global_);
                
                var types = new ComponentTypes
                (new ComponentType[] {
                    typeof(Resource.Initialize),
                    typeof(CubeGridGlobal.BufferData),
                    typeof(CubeGridGlobal.DefualtGridBlankData),
                    typeof(CubeGridGlobal.DefualtGridSolidData),
                    typeof(CubeGridGlobal.InfoData),
                    typeof(Resource.DrawResourceData),
                    typeof(Resource.DrawBufferData),
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag)
                });

                em.AddComponents(ent, types);
            }


            unsafe void setGlobalData_
                (
                    GameObjectConversionSystem gcs_, GameObject global_,
                    int maxCubeInstances_, int maxDrawGridLength_
                )
            {
                var em = gcs_.DstEntityManager;


                var ent = gcs_.GetPrimaryEntity(global_);

                em.SetComponentData(ent,
                    new CubeGridGlobal.InfoData
                    {
                        MaxCubeInstances = maxCubeInstances_,
                        MaxDrawGridLength = maxDrawGridLength_,
                    }
                );
            }


            void setResources_
                (
                    GameObjectConversionSystem gcs_, GameObject global_,
                    Material srcMat_, ComputeShader computeShader_,
                    MarchingCubeAsset asset_, int maxGridLengthInShader_
                )
            {
                var em = gcs_.DstEntityManager;


                var ent = gcs_.GetPrimaryEntity(global_);

                em.SetComponentData(ent,
                    new Resource.DrawResourceData
                    {
                        CubeMaterial = new Material(srcMat_),
                        InstatnceMesh = Resource.CreateMesh(),
                        GridCubeIdSetShader = computeShader_,
                    }
                );

                em.SetComponentData(ent,
                    new Resource.Initialize
                    {
                        Asset = asset_,
                        MaxGridLengthInShader = maxGridLengthInShader_,
                    }
                );
            }


        }

    }
}
