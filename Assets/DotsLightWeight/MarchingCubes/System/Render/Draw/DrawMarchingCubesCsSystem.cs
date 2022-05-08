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
    using DotsLite.MarchingCubes.Data;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Call))]
    [UpdateAfter(typeof(DrawBufferToShaderDataSystem))]
    public partial class DrawMarchingCubeCsSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<Common.DrawShaderResourceData>();
        }

        //int i = 0;
        protected unsafe override void OnUpdate()
        {
            this.Entities
                .WithoutBurst()
                .ForEach((
                    in DrawModel.InstanceCounterData counter,
                    in DrawModel.VectorIndexData offset,
                    in DrawModel.ComputeArgumentsBufferData shaderArg,
                    in DrawModel.GeometryData geom,
                    in CubeDrawModel.MakeCubesShaderResourceData data) =>
                {
                    if (counter.InstanceCounter.Count == 0) return;

                    var cs = data.MakeCubesShader;
                    var mat = geom.Material;

                    //if (cs == null) return;

                    //if (i++ == 0)
                    {
                        data.CubeInstances.Buffer.SetCounterValue(0);// これ最後のほうがよかったりするかな

                        var vectorOffset = offset.ModelStartIndex;
                        cs.SetInt("BoneVectorOffset", (int)vectorOffset);

                        cs.Dispatch(kernelIndex: 0, counter.InstanceCounter.Count, 1, 1);
                        //Debug.Log(counter.InstanceCounter.Count);
                    }

                    {
                        var vectorOffset = offset.ModelStartIndex;
                        mat.SetInt("BoneVectorOffset", (int)vectorOffset);

                        var mesh = geom.Mesh;
                        var src = data.CubeInstances.Buffer;
                        var dst = shaderArg.InstancingArgumentsBuffer;

                        ComputeBuffer.CopyCount(src, dst, dstOffsetBytes: sizeof(int) * 1);
                        //var arr = new int[5];
                        //dst.GetData(arr);
                        //Debug.Log(arr[1]);

                        var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 5000.0f };//
                        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, dst);//
                    }
                })
                .Run();
        }
    }
}
