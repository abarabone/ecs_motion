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

    public class MarchingCubesGridAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {

        public int3 GridLength;

        public GridFillMode FillMode;
        public enum GridFillMode
        {
            Blank,
            Fill,
        };



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var main = this.gameObject;


            setGridArea_(conversionSystem, main, this.FillMode);

            return;


            void setGridArea_(GameObjectConversionSystem gcs_, GameObject main_, GridFillMode fillMode_)
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
                        Grids = Alloc(gcs_, fillMode_, totalSize),
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


            UnsafeList<CubeGrid.BufferData> Alloc(GameObjectConversionSystem gcs_, GridFillMode fillMode_, int totalSize_)
            {
                var buffer = new UnsafeList<CubeGrid.BufferData>(totalSize_, Allocator.Persistent);
                buffer.length = totalSize_;

                var defaultGrid = fillMode_ == GridFillMode.Fill
                    ? gcs_.GetSingleton<CubeGridGlobal.DefualtGridFillData>().DefaultGrid
                    : gcs_.GetSingleton<CubeGridGlobal.DefualtGridBlankData>().DefaultGrid;

                for(var i=0; i<totalSize_; i++)
                {
                    buffer[i] = defaultGrid;
                }

                return buffer;
            }

        }
    }

}
