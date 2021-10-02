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

namespace DotsLite.Draw
{
    
    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Dependency;

    ////[DisableAutoCreation]
    //[UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //public class BeginDrawCsBarier : EntityCommandBufferSystem
    //{ }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.Render.Draw.Call ) )]
    [UpdateAfter(typeof(DrawBufferToShaderDataSystem))]
    public class DrawMeshCsSystem : SystemBase
    {

        protected override unsafe void OnUpdate()
        {
            this.Entities
                .WithoutBurst()
                .WithNone<DrawModel.ExcludeDrawMeshCsTag>()
                .ForEach(
                    (
                        in DrawModel.InstanceCounterData counter,
                        in DrawModel.VectorIndexData offset,
                        in DrawModel.ComputeArgumentsBufferData shaderArg,
                        in DrawModel.GeometryData geom
                    ) =>
                    {
                        if (counter.InstanceCounter.Count == 0) return;

                        var mesh = geom.Mesh;
                        var mat = geom.Material;
                        var args = shaderArg.InstanceArgumentsBuffer;

                        var vectorOffset = offset.ModelStartIndex;
                        mat.SetInt( "BoneVectorOffset", (int)vectorOffset );

                        var instanceCount = counter.InstanceCounter.Count;
                        var argparams = new IndirectArgumentsForInstancing( mesh, instanceCount );
                        args.SetData( ref argparams );

                        var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
                        Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
                    }
                )
                .Run();
        }

    }
}
