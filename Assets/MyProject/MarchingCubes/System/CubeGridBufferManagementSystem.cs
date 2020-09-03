using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    using MarchingCubes;
    using Abarabone.Draw;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class CubeGridBufferManagementSystem : SystemBase
    {


        protected override void OnCreate()
        {
            base.OnCreate();

            this.Enabled = false;


        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {


            this.Entities
                .ForEach(
                    (
                        in CubeGridArea.BufferData buffer
                    ) =>
                    {

                        if (!buffer.Grids.IsCreated) return;

                        buffer.Grids.Dispose();

                    }
                )
                .Run();


            this.Entities
                .ForEach(
                    (
                        in CubeGridGlobal.BufferData buffer
                    ) =>
                    {
                        if (!buffer.CubeBuffers.IsCreated) return;

                        for(var i=0; i<buffer.CubeBuffers.length; i++)
                        {
                            
                            CubeGridAllocater.Dispose( buffer.CubeBuffers[i] );

                        }

                        buffer.CubeBuffers.Dispose();
                    }
                )
                .Run();



            base.OnDestroy();
        }

    }
}
