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

            var cubeInstances = globaldata.CubeInstances;
            //Debug.Log(instances.CubeInstances.length);
            if (cubeInstances.Length == 0) return;

            var cs = globaldata.GridCubeIdSetShader;
            var mat = globaldata.CubeMaterial;
            var buf = globaldata.DrawResources;

            buf.CubeInstancesBuffer.SetData(cubeInstances.AsArray());


            var gridInstances = globaldata.GridInstances;
            //res.GridBuffer.SetData(this.gridData.AsArray());
            var grids = new Vector4[gridInstances.Length * 2];
            fixed (Vector4* pdst = grids)
            {
                var psrc = (Vector4*)gridInstances.GetUnsafePtr();
                UnsafeUtility.MemCpy(pdst, psrc, gridInstances.Length * 2 * sizeof(float4));
            }
            mat.SetVectorArray("grids", grids);


            var remain = (64 - (cubeInstances.Length & 0x3f)) & 0x3f;
            for (var i = 0; i < remain; i++) cubeInstances.AddNoResize(new CubeInstance { instance = 1 });
            var dargparams = new IndirectArgumentsForDispatch(cubeInstances.Length >> 6, 1, 1);
            var dargs = buf.ArgsBufferForDispatch;
            dargs.SetData(ref dargparams);
            cs.Dispatch(0, cubeInstances.Length >> 6, 1, 1);//


            var mesh = globaldata.DrawResources.mesh;
            var iargs = buf.ArgsBufferForInstancing;

            var instanceCount = cubeInstances.Length;
            var iargparams = new IndirectArgumentsForInstancing(mesh, instanceCount);
            iargs.SetData(ref iargparams);


            var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
            Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, iargs);//
        }
    }
}
