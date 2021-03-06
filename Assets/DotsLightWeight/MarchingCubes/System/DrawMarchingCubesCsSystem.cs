﻿using System.Collections;
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
    using Abarabone.Draw;
    using Abarabone.Utilities;

    //[DisableAutoCreation]
    [UpdateAfter(typeof(BeginDrawCsBarier))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
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
                .ForEach(
                        (
                            ref DotGridArea.OutputCubesData output,
                            in DotGridArea.ResourceData res
                        )
                    =>
                    {
                        var cubeInstances = output.CubeInstances;//globaldata.CubeInstances;
                        //Debug.Log(instances.CubeInstances.length);
                        if (cubeInstances.Length == 0) return;


                        var cs = res.GridCubeIdSetShader;
                        var mat = res.CubeMaterial;
                        var abuf = res.Resources;


                        var gridInstances = output.GridInstances;//globaldata.GridInstances;
                        gridInstances.Length = abuf.GridInstancesBuffer.count;//
                        abuf.GridInstancesBuffer.SetData(gridInstances.AsNativeArray());


                        var remain = (64 - (cubeInstances.Length & 0x3f)) & 0x3f;
                        for (var i = 0; i < remain; i++) cubeInstances.AddNoResize(new CubeInstance { instance = 1 });
                        abuf.CubeInstancesBuffer.SetData(cubeInstances.AsNativeArray());


                        if (cs != null)
                        {
                            var dargparams = new IndirectArgumentsForDispatch(cubeInstances.Length >> 6, 1, 1);
                            var dargs = abuf.ArgsBufferForDispatch;
                            dargs.SetData(ref dargparams);
                            cs.Dispatch(0, cubeInstances.Length >> 6, 1, 1);//
                        }


                        var mesh = globaldata.Resources.mesh;
                        var iargs = abuf.ArgsBufferForInstancing;

                        var instanceCount = cubeInstances.Length;
                        var iargparams = new IndirectArgumentsForInstancing(mesh, instanceCount);
                        iargs.SetData(ref iargparams);


                        var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
                        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, iargs);//


                        //output.GridInstances.Clear();
                        //output.CubeInstances.Clear();
                    }
                )
                .Run();
        }
    }
}
