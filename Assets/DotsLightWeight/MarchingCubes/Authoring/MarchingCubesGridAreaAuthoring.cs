using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Authoring;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Model;
    using DotsLite.MarchingCubes.Data;
    using DotsLite.Draw.Authoring;
    using DotsLite.Utilities;
    using DotsLite.EntityTrimmer.Authoring;

    public class MarchingCubesGridAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public MarchingCubesBitGridAuthoring GridPrefab;
        public MarchingCubesGridStockerAuthoring GridStocker;

        //public MarchingCubesAsset MarchingCubesAsset;


        public int3 GridLength;
        public float3 UnitScale;

        public GridFillMode FillMode;

        [Header("DefaultCollisionFilter")]
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            if (!this.isActiveAndEnabled) return;

            referencedPrefabs.Add(this.GridPrefab.gameObject);
            referencedPrefabs.Add(this.GridStocker.gameObject);
        }

        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


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
                    //typeof(BitGridArea.InfoData),
                    //typeof(BitGridArea.InfoWorkData),
                    typeof(BitGridArea.GridInstructionIdData),
                    //typeof(BitGridArea.DrawModelLinkData),
                    //typeof(BitGridArea.PoolLinkData),
                    typeof(BitGridArea.BitGridPrefabData),
                    typeof(BitGridArea.InitializeData),

                    typeof(Marker.Rotation),
                    typeof(Marker.Translation),
                });
                em.AddComponents(ent, types);


                var glen = this.GridLength;
                var gtotal = glen.x * glen.y * glen.z;

                em.SetComponentData(ent, new BitGridArea.GridTypeData
                {
                    GridType = this.GridPrefab.GridType,
                });

                var unitScale = (float3)(1.0 / ((double3)this.UnitScale));
                var gridScale = (float3)(1.0 / ((double3)this.UnitScale * 32));
                var extents = (float3)this.transform.position - (this.GridLength * gridScale) / 2;
                em.SetComponentData(ent, new BitGridArea.UnitDimensionData
                {
                    LeftTopFrontPosition = extents.As_float4(),// extents にして、tf する必要があるかも
                    GridScaleR = gridScale.As_float4(),
                    UnitScaleR = UnitScale.As_float4(),
                    GridLength = new int4(this.GridLength, 0),
                    GridSpan = new int4(1, this.GridLength.x * this.GridLength.z, this.GridLength.x, 0),
                    UnitOnEdge = this.GridPrefab.GridType.ToInt4(),
                });

                //em.SetComponentData(ent, new BitGridArea.InfoData
                //{
                //    GridLength = this.GridLength,
                //});
                //em.SetComponentData(ent, new BitGridArea.InfoWorkData
                //{
                //    GridSpan = new int3(1, this.GridLength.x * this.GridLength.z, this.GridLength.x),
                //});

                //em.SetComponentData(ent, new Marker.Rotation
                //{
                //    Value = this.transform.rotation,
                //});
                //em.SetComponentData(ent, new Marker.Translation
                //{
                //    Value = this.transform.position,
                //});

                //em.SetComponentData(ent, new BitGridArea.DrawModelLinkData
                //{
                //    DrawModelEntity = gcs.GetPrimaryEntity(this.transform.parent)
                //});
                //em.SetComponentData(ent, new BitGridArea.PoolLinkData
                //{
                //    PoolEntity = gcs.GetPrimaryEntity(this.GridStocker)
                //});
                var unitOnEdge = this.GridPrefab.GridType.ToInt4();Debug.Log(unitOnEdge);
                var bitLengthBufferLength = unitOnEdge.y * unitOnEdge.z / (32/unitOnEdge.x);
                //    this.GridPrefab.GridType switch
                //{
                //    BitGridType.Grid16x16x16 => 16 * 16 / (32 / 16),
                //    BitGridType.Grid32x32x32 => 32 * 32 / (32 / 32),
                //    _ => default,
                //};
                //var bitBufferOffset = this
                //    .GetComponentInParent<MarchingCubesDrawModelAuthoring>()
                //    .GetComponentsInChildren<MarchingCubesGridAreaAuthoring>()
                //    .TakeWhile(x => x != this)
                //    .Select(x => x.GridLength.x * x.GridLength.y * x.GridLength.z * bitLengthBufferLength)
                //    .Sum();
                //Debug.Log($"{this.name} {bitBufferOffset}");
                em.SetComponentData(ent, new BitGridArea.BitGridPrefabData
                {
                    Prefab = gcs.GetPrimaryEntity(this.GridPrefab),
                    PoolEntity = gcs.GetPrimaryEntity(this.GridStocker),
                    DrawModelEntity = gcs.GetPrimaryEntity(this.transform.parent),
                    BitLineBufferLength = bitLengthBufferLength,
                    //BitLineBufferOffset = bitBufferOffset,

                    //DefaultGridEntity = gcs.GetPrimaryEntity(this.GridPrefab),// 暫定でプレハブを
                });

                var common = this.GetComponentInParent<MarchingCubesCommonAuthoring>();
                var isSameDefault =
                    this.BelongsTo.Value == common.BelongsTo.Value
                    &&
                    this.CollidesWith.Value == common.CollidesWith.Value;
                //em.SetComponentData(ent, new BitGridArea.UnitCubeColliderAssetData
                //{
                //    IsReferenceDefault = isSameDefault,
                //    CubeColliders = isSameDefault
                //        ? em.GetComponentData<Common.UnitCubeColliderAssetData>(gcs.GetPrimaryEntity(common)).CubeColliders
                //        : common.MarchingCubesAsset.CreateCubeColliders(this.CollisionFilter)
                //});
                if (!isSameDefault)
                {
                    using var cubes = common.MarchingCubesAsset.CreateCubeColliders(new Unity.Physics.CollisionFilter
                    {
                        BelongsTo = this.BelongsTo.Value,
                        CollidesWith = this.CollidesWith.Value,
                    });

                    var buffer = em.AddBuffer<UnitCubeColliderAssetData>(ent);
                    //var buffer = em.GetBuffer<UnitCubeColliderAssetData>(ent);
                    buffer.Capacity = cubes.Length;
                    buffer.CopyFrom(cubes);
                }

                em.SetComponentData(ent, new BitGridArea.InitializeData
                {
                    gridLength = this.GridLength,

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
