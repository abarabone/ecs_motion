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

namespace DotsLite.MarchingCubes.another.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Model;
    using DotsLite.MarchingCubes.another;

    public class MarchingCubesBitGridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public DotGridType GridType = DotGridType.DotGrid32x32x32;



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createDotGrid_(conversionSystem, entity, this.GridType);

            return;


            static void createDotGrid_(GameObjectConversionSystem gcs, Entity ent, DotGridType gridtype)
            {
                var em = gcs.DstEntityManager;

                var types = new List<ComponentType>()
                {
                    typeof(BitGrid.GridTypeData),
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
                types.Add(gridtype switch
                {
                    DotGridType.DotGrid32x32x32 => typeof(BitGrid._32x32x32.BitLinesData),
                    DotGridType.DotGrid16x16x16 => typeof(BitGrid._16x16x16.BitLinesData),
                    _ => default,
                });
                em.AddComponents(ent, new ComponentTypes(types.ToArray()));


                em.SetComponentData(ent, new BitGrid.GridTypeData
                {
                    UnitOnEdge = (int)gridtype,
                });
                em.SetComponentData(ent, new BitGrid.AmountData
                {
                    BitCount = 0,
                    BitLineBufferSize = gridtype switch
                    {
                        DotGridType.DotGrid16x16x16 => 16 * 16 / (32 / 16),
                        DotGridType.DotGrid32x32x32 => 32 * 32 / (32 / 32),
                        _ => default,
                    }
                });
                //em.SetComponentData(ent, new PhysicsCollider
                //{
                //    Value = BlobAssetReference<Unity.Physics.Collider>.Null,
                //});
                em.SetComponentData(ent, new Collision.Hit.TargetData
                {
                    HitType = gridtype switch
                    {
                        DotGridType.DotGrid32x32x32 => Collision.HitType.marchingCubes32,
                        DotGridType.DotGrid16x16x16 => Collision.HitType.marchingCubes16,
                        _ => default,
                    },
                    MainEntity = ent,
                });
            }
        }

    }

}
