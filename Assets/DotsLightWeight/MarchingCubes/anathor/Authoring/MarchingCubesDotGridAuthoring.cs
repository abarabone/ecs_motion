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

    public class MarchingCubesDotGridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
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
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DrawInstance.WorldBbox),
                    typeof(DotGrid.GridTypeData),
                    //typeof(DotGrid.UnitData),
                    typeof(DotGrid.LocationInAreaData),
                    typeof(DotGrid.ParentAreaData),
                    typeof(DotGrid.UpdateDirtyRangeData),
                    typeof(DotGrid.AmountData),
                    //typeof(Unity.Physics.PhysicsCollider),
                    typeof(Collision.Hit.TargetData),
                };
                types.Add(gridtype switch
                {
                    DotGridType.DotGrid32x32x32 => typeof(DotGrid._32x32x32.ContentsData),
                    DotGridType.DotGrid16x16x16 => typeof(DotGrid._16x16x16.ContentsData),
                    _ => default,
                });
                em.AddComponents(ent, new ComponentTypes(types.ToArray()));

                em.SetComponentData(ent, new DotGrid.GridTypeData
                {
                    UnitOnEdge = (int)gridtype,
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
