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

namespace Abarabone.MarchingCubes.Authoring
{
    using Abarabone.Draw;
    using Abarabone.Model;

    public class MarchingCubesGridAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public int3 GridLength;

        public GridFillMode FillMode;



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var global = this.GetComponentInParent<MarchingCubesGlobalDataAuthoring>().gameObject;
            var area = this.gameObject;

            setGridArea_(conversionSystem, global, area, this.FillMode);

            return;


            void setGridArea_
                (GameObjectConversionSystem gcs_, GameObject global_, GameObject area_, GridFillMode fillMode_)
            {
                var em = gcs_.DstEntityManager;

                var ent = gcs_.GetPrimaryEntity(area_);
                var types = new ComponentTypes(
                    new ComponentType[]
                    {
                        typeof(CubeGridArea.InitializeData),
                        typeof(CubeGridArea.BufferData),
                        typeof(CubeGridArea.InfoData),
                        typeof(CubeGridArea.InfoTempData),
                        typeof(Rotation),
                        typeof(Translation)
                    }
                );
                em.AddComponents(ent, types);


                var wholeLength = this.GridLength + 2;
                var totalSize = wholeLength.x * wholeLength.y * wholeLength.z;

                em.SetComponentData(ent,
                    new CubeGridArea.BufferData
                    {
                        Grids = allocGridArea_(totalSize, fillMode_),
                    }
                );
                em.SetComponentData(ent,
                    new CubeGridArea.InitializeData
                    {
                        FillMode = fillMode_,
                    }
                );
                em.SetComponentData(ent,
                    new CubeGridArea.InfoData
                    {
                        GridLength = this.GridLength,
                        GridWholeLength = wholeLength,
                    }
                );
                em.SetComponentData(ent,
                    new Rotation
                    {
                        Value = this.transform.rotation,
                    }
                );
                em.SetComponentData(ent,
                    new Translation
                    {
                        Value = this.transform.position,
                    }
                );


                UnsafeList<CubeGrid32x32x32Unsafe> allocGridArea_(int totalSize_, GridFillMode fillMode)
                {
                    var buffer = new UnsafeList<CubeGrid32x32x32Unsafe>(totalSize, Allocator.Persistent);
                    buffer.length = totalSize_;

                    var gent = gcs_.GetPrimaryEntity(global_);
                    var defaultGrids = em.GetBuffer<CubeGridGlobal.DefualtGridData>(gent);
                    var defaultGrid = defaultGrids.GetDefaultGrid(fillMode);

                    for (var i = 0; i < totalSize; i++)
                    {
                        buffer[i] = defaultGrid;
                    }

                    return buffer;
                }
            }


        }
    }

}
