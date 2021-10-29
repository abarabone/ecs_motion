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

namespace DotsLite.MarchingCubes.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Model;

    public class MarchingCubesGlobalAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        //public int MaxCubeInstances;
        [Range(0, 512)]
        public int MaxGridInstances;
        public int MaxFreeGrids;

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
                    typeof(Global.CommonData),
                    typeof(Global.Work32Data),
                    typeof(Global.InitializeData),
                });
                em.AddComponents(ent, types);

                em.SetComponentData(ent, new Global.InitializeData
                {
                    maxFreeGrids = this.MaxFreeGrids,
                    maxGridInstances = this.MaxGridInstances,
                    asset = this.MarchingCubesAsset,
                });
                em.SetComponentData(ent, new Global.CommonData
                {
                    
                });
                em.SetComponentData(ent, new Global.Work32Data
                {
                    Assset = this.MarchingCubesAsset.ConvertToBlobData(),
                });
            }

        }
    }
}