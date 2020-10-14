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

    public class MarchingCubesAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public int MaxCubeInstances;
        public int MaxGridInstances;
        public int MaxFreeGrids;

        public MarchingCubeAsset MarchingCubesAsset;
        public Material SrcMaterial;
        public ComputeShader GridCubeIdSetShader;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createDataEntity_(conversionSystem, this.gameObject);

            return;


            unsafe void createDataEntity_(GameObjectConversionSystem gcs_, GameObject global_)
            {
                var em = gcs_.DstEntityManager;

                var ent = gcs_.GetPrimaryEntity(global_);
                em.AddComponentData(ent,
                    new MarchingCubeGlobalData()
                        .Init(this.MaxCubeInstances, this.MaxGridInstances, this.MaxFreeGrids, this.MarchingCubesAsset)
                );
            }

        }
    }
}