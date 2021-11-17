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

namespace DotsLite.MarchingCubes.Authoring.another
{
    using DotsLite.Draw;
    using DotsLite.Model;
    using DotsLite.MarchingCubes.Data;
    using DotsLite.MarchingCubes.Data.Resource;

    public class MarchingCubesCommonAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        //public DotGridType GridType = DotGridType.DotGrid32x32x32;


        //public int MaxCubeInstances;
        //[Range(0, 512)]
        //public int MaxGridInstances;
        //public int MaxFreeGrids;

        public MarchingCubesAsset MarchingCubesAsset;
        //public Material SrcMaterial;
        //public ComputeShader GridCubeIdSetShader;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createDataEntity_(conversionSystem, entity);

            return;


            unsafe void createDataEntity_(GameObjectConversionSystem gcs, Entity ent)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(Common.DrawShaderResourceData),
                    typeof(Common.InitializeData),
                });
                em.AddComponents(ent, types);


                em.SetComponentData(ent, new Common.DrawShaderResourceData { });
                em.SetComponentData(ent, new Common.InitializeData
                {
                    asset = this.MarchingCubesAsset
                });
            }

        }
    }
}