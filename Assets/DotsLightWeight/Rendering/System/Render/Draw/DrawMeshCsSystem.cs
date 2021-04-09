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

namespace Abarabone.Draw
{
    
    using Abarabone.Misc;
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Dependency;

    ////[DisableAutoCreation]
    //[UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //public class BeginDrawCsBarier : EntityCommandBufferSystem
    //{ }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    public class DrawMeshCsSystem : SystemBase, BarrierDependency.IRecievable
    {


        public BarrierDependency.Reciever Reciever { get; } = BarrierDependency.Reciever.Create();



        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override unsafe void OnUpdate()
        {
            this.Reciever.CompleteAllDependentJobs(this.Dependency);

            var nativeBuffer = this.GetSingleton<DrawSystem.NativeTransformBufferData>().Transforms;
            var computeBuffer = this.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;
            computeBuffer.SetData( nativeBuffer.AsNativeArray() );

            //Debug.Log("start");
            //for (var i = 0; i < nativeBuffer.length_; i++)
            //{
            //    Debug.Log($"{i} {nativeBuffer.pBuffer[i]}");
            //}

            this.Entities
                .WithoutBurst()
                .ForEach(
                    (
                        in DrawModel.InstanceCounterData counter,
                        in DrawModel.InstanceOffsetData offset,
                        in DrawModel.ComputeArgumentsBufferData shaderArg,
                        in DrawModel.GeometryData geom
                    ) =>
                    {
                        if (counter.InstanceCounter.Count == 0) return;

                        var mesh = geom.Mesh;
                        var mat = geom.Material;
                        var args = shaderArg.InstanceArgumentsBuffer;
                        
                        var vectorOffset = offset.pVectorOffsetPerModelInBuffer - nativeBuffer.pBuffer;
                        mat.SetInt( "BoneVectorOffset", (int)vectorOffset );
                        //mat.SetInt( "BoneLengthEveryInstance", mesh.bindposes.Length );
                        //mat.SetBuffer( "BoneVectorBuffer", computeBuffer );
                        //Debug.Log($"{mesh.name} {((uint4*)offset.pVectorOffsetPerModelInBuffer)[0]}");
                        //Debug.Log($"{mesh.name} {((uint4*)offset.pVectorOffsetPerModelInBuffer)[6]}");

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
