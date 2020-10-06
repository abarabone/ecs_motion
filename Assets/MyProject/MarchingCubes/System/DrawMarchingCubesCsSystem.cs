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

            this.RequireSingletonForUpdate<DotGridGlobal.InstanceWorkData>();
        }

        protected unsafe override void OnUpdate()
        {

            var instances = this.GetSingleton<DotGridGlobal.InstanceWorkData>();
            //Debug.Log(instances.CubeInstances.length);
            if (instances.CubeInstances.Length == 0) return;

            var res = this.GetSingleton<Resource.DrawResourceData>();
            var buf = this.GetSingleton<Resource.DrawBufferData>().DrawResources;

            buf.CubeInstancesBuffer.SetData(instances.CubeInstances.AsNativeArray());


            //res.GridBuffer.SetData(this.gridData.AsArray());
            var grids = new Vector4[instances.GridInstances.Length * 2];
            fixed (Vector4* pdst = grids)
            {
                var psrc = (Vector4*)instances.GridInstances.Ptr;
                UnsafeUtility.MemCpy(pdst, psrc, instances.GridInstances.Length * 2 * sizeof(float4));
            }
            res.CubeMaterial.SetVectorArray("grids", grids);


            var remain = (64 - (instances.CubeInstances.Length & 0x3f)) & 0x3f;
            for (var i = 0; i < remain; i++) instances.CubeInstances.AddNoResize(new CubeInstance { instance = 1 });
            var dargparams = new IndirectArgumentsForDispatch(instances.CubeInstances.Length >> 6, 1, 1);
            var dargs = buf.ArgsBufferForDispatch;
            dargs.SetData(ref dargparams);
            res.GridCubeIdSetShader.Dispatch(0, instances.CubeInstances.Length >> 6, 1, 1);//


            var mesh = buf.mesh;//res.CubeMesh;
            var mat = res.CubeMaterial;
            var iargs = buf.ArgsBufferForInstancing;

            var instanceCount = instances.CubeInstances.Length;
            var iargparams = new IndirectArgumentsForInstancing(mesh, instanceCount);
            iargs.SetData(ref iargparams);


            var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
            Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, iargs);//
        }
    }
}
