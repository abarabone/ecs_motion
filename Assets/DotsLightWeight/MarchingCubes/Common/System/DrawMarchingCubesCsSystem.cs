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
    using DotsLite.MarchingCubes;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Call))]
    [UpdateAfter(typeof(DrawBufferToShaderDataSystem))]
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

            var gbuf = globaldata.ShaderResources;

            this.Entities
                .WithoutBurst()
                .ForEach((
                    in DrawModel.InstanceCounterData counter,
                    in DrawModel.VectorIndexData offset,
                        in DrawModel.ComputeArgumentsBufferData shaderArg,
                        in DrawModel.GeometryData geom,
                    in DotGridArea.ResourceGpuModeData res) =>
                {
                    //var cubeInstances = output.CubeInstances;//globaldata.CubeInstances;
                    ////Debug.Log(instances.CubeInstances.length);
                    //if (cubeInstances.Length == 0) return;
                    if (counter.InstanceCounter.Count == 0) return;

                    var cs = res.GridToCubeShader;
                    //var mat = geom.Material;
                    var mat = res.CubeMaterial;
                    var buf = res.ShaderResources;

                    if (cs == null) return;


                    {
                        //var gridInstances = output.GridInstances;//globaldata.GridInstances;
                        //gridInstances.Length = abuf.GridInstancesBuffer.count;//
                        //abuf.GridInstancesBuffer.SetData(gridInstances.AsNativeArray());

                        //var remain = (64 - (cubeInstances.Length & 0x3f)) & 0x3f;
                        //for (var i = 0; i < remain; i++) cubeInstances.AddNoResize(new CubeInstance { instance = 1 });
                        //abuf.CubeInstancesBuffer.SetData(cubeInstances.AsNativeArray());

                        buf.CubeInstances.Buffer.SetCounterValue(0);

                        var vectorOffset = offset.ModelStartIndex;
                        cs.SetInt("BoneVectorOffset", (int)vectorOffset);

                        cs.Dispatch(kernelIndex: 0, counter.InstanceCounter.Count, 1, 1);
                        //Debug.Log(counter.InstanceCounter.Count);
                    }

                    {
                        var vectorOffset = offset.ModelStartIndex;
                        mat.SetInt("BoneVectorOffset", (int)vectorOffset);

                        var mesh = globaldata.ShaderResources.mesh;
                        var iargs = buf.CubeInstancingArgs;
                        //var mesh = geom.Mesh;
                        //var iargs = shaderArg.InstanceArgumentsBuffer;
                        var src = buf.CubeInstances;

                        //var xs = (
                        //    from x in Enumerable.Range(0, 32)
                        //    from z in Enumerable.Range(0, 32)
                        //    select CubeUtility.ToCubeInstance(x, 0, z, 0, (uint)(x+z))
                        //).ToArray();
                        //buf.CubeInstances.Buffer.SetData(xs, 0, 0, buf.CubeInstances.Buffer.count);

                        //var instanceCount = 0;// src.Buffer.count;//xs.Count();// cubeInstances.Length;
                        //var iargparams = new IndirectArgumentsForInstancing(mesh, instanceCount);
                        //iargs.Buffer.SetData(ref iargparams);
                        ComputeBuffer.CopyCount(src.Buffer, iargs.Buffer, dstOffsetBytes: sizeof(int) * 1);
                        //var arr = new int[5];
                        //iargs.Buffer.GetData(arr);
                        //Debug.Log(arr[1]);

                        var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
                        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, iargs.Buffer);//
                    }
                })
                .Run();
        }
    }
}
