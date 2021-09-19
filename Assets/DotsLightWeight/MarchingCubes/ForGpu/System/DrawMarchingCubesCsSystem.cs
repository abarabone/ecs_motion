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

namespace DotsLite.MarchingCubes.Gpu
{
    using DotsLite.Draw;
    using DotsLite.Utilities;

    //[DisableAutoCreation]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateAfter(typeof(DrawMeshCsSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Call))]
    public class DrawMarchingCubeCsSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<MarchingCubeGlobalData>();
        }

        protected unsafe override void OnUpdate()
        {
            var globaldata = this.GetSingleton<MarchingCubeGlobalData>();

            var gbuf = globaldata.Resources;

            this.Entities
                .WithoutBurst()
                .ForEach((
                    in DotGridArea.ResourceGpuModeData res) =>
                {
                    var cubeInstances = output.CubeInstances;//globaldata.CubeInstances;
                    ////Debug.Log(instances.CubeInstances.length);
                    //if (cubeInstances.Length == 0) return;


                    var cs = res.GridToCubeShader;
                    var mat = res.CubeMaterial;
                    var abuf = res.Resources;


                    //var gridInstances = output.GridInstances;//globaldata.GridInstances;
                    //gridInstances.Length = abuf.GridInstancesBuffer.count;//
                    //abuf.GridInstancesBuffer.SetData(gridInstances.AsNativeArray());


                    //var remain = (64 - (cubeInstances.Length & 0x3f)) & 0x3f;
                    //for (var i = 0; i < remain; i++) cubeInstances.AddNoResize(new CubeInstance { instance = 1 });
                    //abuf.CubeInstancesBuffer.SetData(cubeInstances.AsNativeArray());


                    //if (cs != null)
                    //{
                    //    var dargparams = new IndirectArgumentsForDispatch(cubeInstances.Length >> 6, 1, 1);
                    //    var dargs = abuf.ArgsBufferForDispatch;
                    //    dargs.SetData(ref dargparams);
                    //    cs.Dispatch(0, cubeInstances.Length >> 6, 1, 1);//
                    //}


                    var mesh = globaldata.Resources.mesh;
                    var iargs = abuf.CubeInstancingArgs;

                    //var instanceCount = cubeInstances.Length;
                    //var iargparams = new IndirectArgumentsForInstancing(mesh, instanceCount);
                    //iargs.Buffer.SetData(ref iargparams);
                    ComputeBuffer.CopyCount(, iargs.Buffer, sizeof(int) * 1);

                    var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
                    Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, iargs.Buffer);//
                })
                .Run();
        }
    }
}
