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
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;

namespace DotsLite.MarchingCubes.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Model;
    using DotsLite.MarchingCubes.Data;

    public class MarchingCubesBitGridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public BitGridType GridType = BitGridType.Grid32x32x32;



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createDotGrid_(conversionSystem, entity, this.GridType);

            return;


            static void createDotGrid_(GameObjectConversionSystem gcs, Entity ent, BitGridType gridtype)
            {
                var em = gcs.DstEntityManager;

                var types = new List<ComponentType>()
                {
                    typeof(BitGrid.GridTypeData),
                    typeof(BitGrid.BitLinesData),
                    //typeof(BitGrid.BufferLengthData),
                    typeof(BitGrid.AmountData),
                    typeof(BitGrid.LocationInAreaData),
                    typeof(BitGrid.UpdateDirtyRangeData),
                    typeof(BitGrid.ParentAreaData),
                    //typeof(Unity.Physics.PhysicsCollider),
                    typeof(Collision.Hit.TargetData),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DrawInstance.WorldBbox),
                };
                em.AddComponents(ent, new ComponentTypes(types.ToArray()));


                em.SetComponentData(ent, new BitGrid.GridTypeData
                {
                    UnitOnEdge = (int)gridtype,
                });
                //em.SetComponentData(ent, new BitGrid.BufferLengthData
                //{
                //    BitLineBufferLength = gridtype switch
                //    {
                //        BitGridType.Grid16x16x16 => 16 * 16 / (32 / 16),
                //        BitGridType.Grid32x32x32 => 32 * 32 / (32 / 32),
                //        _ => default,
                //    }
                //});
                em.SetComponentData(ent, new BitGrid.AmountData
                {
                    BitCount = 0,
                });
                //em.SetComponentData(ent, new PhysicsCollider
                //{
                //    Value = BlobAssetReference<Unity.Physics.Collider>.Null,
                //});
                em.SetComponentData(ent, new Collision.Hit.TargetData
                {
                    HitType = gridtype switch
                    {
                        BitGridType.Grid32x32x32 => Collision.HitType.marchingCubes32,
                        BitGridType.Grid16x16x16 => Collision.HitType.marchingCubes16,
                        _ => default,
                    },
                    MainEntity = ent,
                });
            }
        }

    }

}
