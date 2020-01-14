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

using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;
using Abss.Utilities;

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    public class BeginDrawCsBarier : EntityCommandBufferSystem
    { }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    public class DrawMeshCsSystem : JobComponentSystem
    {


        protected override unsafe JobHandle OnUpdate( JobHandle inputDeps )
        {
            var nativeBuffer = this.GetSingleton<DrawSystemNativeTransformBufferData>().Transforms;
            var computeBuffer = this.GetSingleton<DrawSystemComputeTransformBufferData>().Transforms;
            computeBuffer.SetData( nativeBuffer.AsNativeArray() );

            //Debug.Log( "start" );
            //for(var i=0; i<nativeBuffer.length_; i++ )
            //{
            //    Debug.Log( $"{i} {nativeBuffer.pBuffer[i]}" );
            //}

            this.Entities
                .WithoutBurst()
                .ForEach(
                    (
                        in DrawModelInstanceCounterData counter,
                        in DrawModelInstanceOffsetData offset,
                        in DrawModelComputeArgumentsBufferData shaderArg,
                        in DrawModelGeometryData geom
                    ) =>
                    {
                        var mesh = geom.Mesh;
                        var mat = geom.Material;
                        var args = shaderArg.InstanceArgumentsBuffer;
                        
                        var vectorOffset = offset.pVectorOffsetInBuffer - nativeBuffer.pBuffer;
                        mat.SetInt( "BoneVectorOffset", (int)vectorOffset );
                        //mat.SetInt( "BoneLengthEveryInstance", mesh.bindposes.Length );
                        //mat.SetBuffer( "BoneVectorBuffer", computeBuffer );

                        var instanceCount = counter.InstanceCounter.Count;
                        var argparams = new IndirectArgumentsForInstancing( mesh, instanceCount );
                        args.SetData( ref argparams );

                        var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
                        Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
                    }
                )
                .Run();
            
            return inputDeps;
        }

    }
}
