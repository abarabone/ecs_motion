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

    public class MarchingCubesGridAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public int3 GridLength;

        public GridFillMode FillMode;

        public MarchingCubesGlobalDataAuthoring GlobalData;



        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.GlobalData.gameObject);
        }



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var main = this.gameObject;
            var global = this.GlobalData.gameObject;


            setGridArea_(conversionSystem, main, global, this.FillMode);

            return;


            void setGridArea_(GameObjectConversionSystem gcs_, GameObject main_, GameObject global_, GridFillMode fillMode_)
            {
                var em = gcs_.DstEntityManager;

                var ent = gcs_.GetPrimaryEntity(main_);
                var types = new ComponentTypes
                (
                    typeof(CubeGridArea.BufferData),
                    typeof(CubeGridArea.InfoData),
                    typeof(Rotation),
                    typeof(Translation)
                );
                em.AddComponents(ent, types);


                var wholeLength = this.GridLength + 2;
                var totalSize = wholeLength.x * wholeLength.y * wholeLength.z;

                em.SetComponentData(ent,
                    new CubeGridArea.BufferData
                    {
                        Grids = AllocGridArea(gcs_, global_, totalSize, fillMode_),
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
            }


            UnsafeList<CubeGrid32x32x32Unsafe> AllocGridArea
                (GameObjectConversionSystem gcs_, GameObject global_, int totalSize_, GridFillMode fillMode_)
            {
                var buffer = new UnsafeList<CubeGrid32x32x32Unsafe>(totalSize_, Allocator.Persistent);
                buffer.length = totalSize_;

                var em = gcs_.DstEntityManager;

                var globalEnt = gcs_.GetPrimaryEntity(global_);
                var defaultGrid = fillMode_ == GridFillMode.Solid
                    ? em.GetComponentData<CubeGridGlobal.DefualtGridSolidData>(globalEnt).DefaultGrid
                    : em.GetComponentData<CubeGridGlobal.DefualtGridBlankData>(globalEnt).DefaultGrid;

                for(var i=0; i<totalSize_; i++)
                {
                    buffer[i] = defaultGrid;
                }

                return buffer;
            }

        }
    }

}
