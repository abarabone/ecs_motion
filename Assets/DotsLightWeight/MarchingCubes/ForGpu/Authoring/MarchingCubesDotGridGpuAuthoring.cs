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

namespace DotsLite.MarchingCubes.Gpu.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Model;
    using DotsLite.MarchingCubes;
    using DotsLite.MarchingCubes.Authoring;

    public class MarchingCubesDotGridGpuAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {





        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {


            createDotGrid_(conversionSystem, this.gameObject);

            return;


            static void createDotGrid_(GameObjectConversionSystem gcs, GameObject grid)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.GetPrimaryEntity(grid);
                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(DotGrid.UnitData),
                });
                em.AddComponents(ent, types);

                //em.SetComponentData(ent, new DotGrid.UnitData
                //{
                //    Unit =
                //});
            }


        }
    }

}
