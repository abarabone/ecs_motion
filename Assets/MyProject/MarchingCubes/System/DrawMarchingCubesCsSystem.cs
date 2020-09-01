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
    using Abarabone.Draw;

    //[DisableAutoCreation]
    [UpdateAfter(typeof(BeginDrawCsBarier))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    public class DrawMarchingCubeCsSystem : SystemBase
    {
        protected unsafe override void OnUpdate()
        {

            this.Entities
                .ForEach(
                    (in Grid.GridBufferData buf) =>
                    {
                        
                    }
                )
                .ScheduleParallel();


            //var res = this.GetSingleton<Resource.DrawResourceData>();
            //var gridbuf = this.GetSingleton<Grid.GridBufferData>();
            //var resbuf = this.GetSingleton<Resource.DrawBufferData>();


            //resbuf.CubeInstancesBuffer.SetData(gridbuf.cubeInstances.AsArray());

            ////res.GridBuffer.SetData( this.gridData.AsArray() );
            //var grids = new Vector4[gridbuf.gridData.Length * 2];
            //fixed (Vector4* pdst = grids)
            //{
            //    var psrc = (Vector4*)gridbuf.gridData.GetUnsafeReadOnlyPtr();
            //    UnsafeUtility.MemCpy(pdst, psrc, gridbuf.gridData.Length * 2 * sizeof(float4));
            //}
            //res.CubeMaterial.SetVectorArray("grids", grids);


            //var remain = (64 - (gridbuf.cubeInstances.Length & 0x3f)) & 0x3f;
            //for (var i = 0; i < remain; i++) gridbuf.cubeInstances.AddNoResize(new CubeInstance { instance = 1 });
            //var dargparams = new IndirectArgumentsForDispatch(gridbuf.cubeInstances.Length >> 6, 1, 1);
            //var dargs = resbuf.ArgsBufferForDispatch;
            //dargs.SetData(ref dargparams);
            //res.SetGridCubeIdShader.Dispatch(0, gridbuf.cubeInstances.Length >> 6, 1, 1);//


            //var mesh = res.CubeMesh;
            //var mat = res.CubeMaterial;
            //var iargs = resbuf.ArgsBufferForInstancing;

            //var instanceCount = gridbuf.cubeInstances.Length;
            //var iargparams = new IndirectArgumentsForInstancing(mesh, instanceCount);
            //iargs.SetData(ref iargparams);


            //var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
            //Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, iargs);//
        }
    }
}
