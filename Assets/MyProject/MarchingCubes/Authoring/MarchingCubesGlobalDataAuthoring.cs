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
    using System;

    public class MarchingCubesGlobalDataAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public int MaxCubeInstances;
        public int MaxDrawGridLength;




        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {


            setGlobalData_(conversionSystem, this.gameObject);

            return;


            unsafe void setGlobalData_(GameObjectConversionSystem gcs_, GameObject global_)
            {
                var em = gcs_.DstEntityManager;

                var ent = gcs_.GetPrimaryEntity(global_);
                var types = new ComponentTypes
                (
                    typeof(CubeGridGlobal.BufferData),
                    typeof(CubeGridGlobal.DefualtGridBlankData),
                    typeof(CubeGridGlobal.DefualtGridSolidData),
                    typeof(CubeGridGlobal.InfoData)
                );
                em.AddComponents(ent, types);


                var buffer = new UnsafeList<UIntPtr>(this.MaxCubeInstances, Allocator.Persistent);
                var solid = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Solid);
                var blank = CubeGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Blank);
                buffer.Add((UIntPtr)solid.pUnits);
                buffer.Add((UIntPtr)blank.pUnits);


                em.SetComponentData(ent,
                    new CubeGridGlobal.BufferData
                    {
                        CubeBuffers = buffer,
                    }
                );
                em.SetComponentData(ent,
                    new CubeGridGlobal.DefualtGridSolidData
                    {
                        DefaultGrid = solid,
                    }
                );
                em.SetComponentData(ent,
                    new CubeGridGlobal.DefualtGridBlankData
                    {
                        DefaultGrid = blank,
                    }
                );
                em.SetComponentData(ent,
                    new CubeGridGlobal.InfoData
                    {
                        MaxCubeInstances = this.MaxCubeInstances,
                        MaxDrawGridLength = this.MaxDrawGridLength,
                    }
                );
            }


        }

    }
}
