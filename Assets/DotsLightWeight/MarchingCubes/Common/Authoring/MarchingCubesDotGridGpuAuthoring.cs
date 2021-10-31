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
    using DotsLite.MarchingCubes;
    using DotsLite.MarchingCubes.Authoring;

    public class MarchingCubesDotGridGpuAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public DotGridType GridType = DotGridType.DotGrid32x32x32;



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            switch (this.GridType)
            {
                case DotGridType.DotGrid32x32x32:
                    createDotGrid_<DotGrid32x32x32>(conversionSystem, entity);
                    break;

                case DotGridType.DotGrid16x16x16:
                    createDotGrid_<DotGrid16x16x16>(conversionSystem, entity);
                    break;

                default:
                    break;
            }

            return;


            static void createDotGrid_<TGrid>(GameObjectConversionSystem gcs, Entity ent)
                where TGrid : struct, IDotGrid<TGrid>
            {
                var em = gcs.DstEntityManager;

                var types = new List<ComponentType>()
                {
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DrawInstance.WorldBbox),
                    DotGrid.TypeOf_UnitData<TGrid>(),//typeof(DotGrid.UnitData),
                    typeof(DotGrid.IndexData),
                    typeof(DotGrid.ParentAreaData),
                    typeof(DotGrid.UpdateDirtyRangeData),
                    //typeof(Unity.Physics.PhysicsCollider),
                    typeof(Collision.Hit.TargetData),
                };
                em.AddComponents(ent, new ComponentTypes(types.ToArray()));

                //em.SetComponentData(ent, new PhysicsCollider
                //{
                //    Value = BlobAssetReference<Unity.Physics.Collider>.Null,
                //});
                em.SetComponentData(ent, new Collision.Hit.TargetData
                {
                    HitType = Collision.HitType.marchingCubes32,
                    MainEntity = ent,
                });
            }


        }
    }

}
