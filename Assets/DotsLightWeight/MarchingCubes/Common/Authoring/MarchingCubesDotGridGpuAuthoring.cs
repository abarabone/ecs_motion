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





        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {


            createDotGrid_(conversionSystem, entity);

            return;


            static void createDotGrid_(GameObjectConversionSystem gcs, Entity ent)
            {
                var em = gcs.DstEntityManager;

                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DrawInstance.WorldBbox),
                    typeof(DotGrid.UnitData<DotGrid32x32x32>),
                    typeof(DotGrid.ParentAreaData),
                    //typeof(PhysicsCollider),
                    typeof(DotGrid.UpdateDirtyRangeData),
                    //typeof(Unity.Physics.PhysicsCollider),
                    typeof(Collision.Hit.TargetData),
                });
                em.AddComponents(ent, types);

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
