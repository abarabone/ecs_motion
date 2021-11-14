﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes.another.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Model;
    using DotsLite.MarchingCubes.another.Data;
    using DotsLite.Draw.Authoring;
    using DotsLite.Utilities;

    public class MarchingCubesGridAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public MarchingCubesBitGridAuthoring GridPrefab;
        public MarchingCubesGridStockerAuthoring GridStocker;

        public MarchingCubesAsset MarchingCubesAsset;


        public int3 GridLength;
        public float3 UnitScale;

        public GridFillMode FillMode;



        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.GridPrefab.gameObject);
            referencedPrefabs.Add(this.GridStocker.gameObject);
        }

        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initGridArea_(conversionSystem, entity);

            return;


            void initGridArea_(GameObjectConversionSystem gcs, Entity ent)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(BitGridArea.GridTypeData),
                    typeof(BitGridArea.GridLinkData),
                    typeof(BitGridArea.UnitDimensionData),
                    typeof(BitGridArea.InfoData),
                    typeof(BitGridArea.InfoWorkData),
                    typeof(BitGridArea.GridInstructionIdData),
                    typeof(BitGridArea.GridInstructionIdSeedData),
                    typeof(Rotation),
                    typeof(Translation),
                    typeof(BitGridArea.DrawModelLinkData),
                    typeof(BitGridArea.PoolLinkData),
                    typeof(BitGridArea.DotGridPrefabData),
                    typeof(BitGridArea.InitializeData)
                });
                em.AddComponents(ent, types);


                var glen = this.GridLength;
                var gtotal = glen.x * glen.y * glen.z;

                em.SetComponentData(ent, new BitGridArea.GridTypeData
                {
                    UnitOnEdge = (int)this.GridPrefab.GridType,
                });

                var unitScale = (float3)(1.0 / ((double3)this.UnitScale));
                var gridScale = (float3)(1.0 / ((double3)this.UnitScale * 32));
                var extents = (float3)this.transform.position - (this.GridLength * gridScale) / 2;
                em.SetComponentData(ent, new BitGridArea.UnitDimensionData
                {
                    LeftTopFrontPosition = extents.As_float4(),// extents にして、tf する必要があるかも
                    GridScaleR = gridScale.As_float4(),
                    UnitScaleR = UnitScale.As_float4(),
                });

                em.SetComponentData(ent, new BitGridArea.InfoData
                {
                    GridLength = this.GridLength,
                });
                em.SetComponentData(ent, new BitGridArea.InfoWorkData
                {
                    GridSpan = new int3(1, this.GridLength.x * this.GridLength.z, this.GridLength.x),
                });

                em.SetComponentData(ent, new Rotation
                {
                    Value = this.transform.rotation,
                });
                em.SetComponentData(ent, new Translation
                {
                    Value = this.transform.position,
                });

                em.SetComponentData(ent, new BitGridArea.DrawModelLinkData
                {
                    DrawModelEntity = gcs.GetPrimaryEntity(this.transform.parent)
                });
                em.SetComponentData(ent, new BitGridArea.PoolLinkData
                {
                    PoolEntity = gcs.GetPrimaryEntity(this.GridStocker)
                });
                em.SetComponentData(ent, new BitGridArea.DotGridPrefabData
                {
                    Prefab = gcs.GetPrimaryEntity(this.GridPrefab)
                });

                em.SetComponentData(ent, new BitGridArea.InitializeData
                {
                    gridLength = this.GridLength
                });
            }


            //void setModelToPrefab_(GameObjectConversionSystem gcs, Entity ent, Entity prefab)
            //{
            //    var em = gcs.DstEntityManager;

            //    //em.AddComponentData(prefab, new DrawInstance.ModelLinkData
            //    //{
            //    //    DrawModelEntityCurrent = ent,
            //    //});
            //    em.AddComponentData(prefab, new BitGrid.ParentAreaData
            //    {
            //        ParentAreaEntity = ent,
            //    });
            //}



        }

    }

}
