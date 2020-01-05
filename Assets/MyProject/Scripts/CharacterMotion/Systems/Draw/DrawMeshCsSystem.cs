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
    [UpdateAfter(typeof( BoneToDrawInstanceSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    public class BeginDrawCsBarier : EntityCommandBufferSystem
    { }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    //[AlwaysUpdateSystem]
    //[DisableAutoCreation]
    [UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    public class DrawMeshCsSystem : JobComponentSystem
    {

        public int MaxInstance = 10000;


        

        public DrawComputeInstanceBufferHolder ComputeBuffers { get; } = new DrawComputeInstanceBufferHolder();

        public DrawNativeInstanceBufferHolder NativeBuffers { get; } = new DrawNativeInstanceBufferHolder();


        // 描画モデルリソース
        DrawMeshResourceHolder resourceHolder = new DrawMeshResourceHolder();

        public DrawMeshResourceHolder GetResourceHolder() => this.resourceHolder;

        


        protected override void OnStartRunning()
        {

            this.ComputeBuffers.Initialize( this.resourceHolder );
            this.NativeBuffers.Initialize( this.resourceHolder );
            
        }

        protected override void OnStopRunning()
        {
            this.resourceHolder.Dispose();
            this.ComputeBuffers.Dispose();
            this.NativeBuffers.Dispose();
        }


        protected override unsafe JobHandle OnUpdate( JobHandle inputDeps )
        {

            var nativeBuffer = this.GetSingleton<DrawSystemNativeTransformBufferData>().Transforms;
            var computeBuffer = this.GetSingleton<DrawSystemComputeTransformBufferData>().Transforms;
            computeBuffer.SetData( nativeBuffer.AsNativeArray() );

            this.Entities
                .WithoutBurst()
                .ForEach(
                    (
                        in DrawModelBoneUnitSizeData boneUnitInfo,
                        in DrawModelInstanceCounterData counter,
                        in DrawModelInstanceOffsetData offset,
                        in DrawModelComputeArgumentsBufferData shaderArg,
                        in DrawModelGeometryData geom
                    ) =>
                    {

                        var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };

                        var mesh = geom.Mesh;
                        var mat = geom.Material;
                        var args = shaderArg.InstanceArgumentsBuffer;

                        var vector

                        //mat.SetBuffer( "BoneVectorBuffer", dstBuffer );
                        //mat.SetInt( "BoneVectorLength", mesh.bindposes.Length * (int)boneType );
                        mat.SetInt( "BoneVectorOffset", offset.VectorOffsetInBuffer );// 毎フレームのセットが必要

                        var instanceCount = counter.InstanceCounter.Count;
                        var argparams = new IndirectArgumentsForInstancing( mesh, instanceCount );
                        args.SetData( ref argparams );

                        Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
                    }
                );
            
            return inputDeps;

            //for( var i = 0; i < this.resourceHolder.Units.Count; i++ )
            //{
            //    var resource = this.resourceHolder.Units[ i ];
            //    var nativebuf = this.NativeBuffers.Units[ i ];
            //    var devicebuf = this.ComputeBuffers.Units[ i ];

            //    var mesh = resource.Mesh;
            //    var mat = resource.Material;
            //    var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
            //    var args = devicebuf.InstanceArgumentsBuffer;

            //    var instanceCount = nativebuf.InstanceCounter.Count;
            //    var argparams = new IndirectArgumentsForInstancing( mesh, instanceCount );
            //    args.SetData( ref argparams );

            //    var boneLength = mesh.bindposes.Length;

            //    var outputVectorCount = instanceCount * boneLength * nativebuf.VectorLengthInBone;
            //    var srcBuffer = this.NativeBuffers.InstanceBoneVectors;//this.tempBufferSystem.TempInstanceBoneVectors;
            //    var dstBuffer = devicebuf.TransformBuffer;
            //    dstBuffer.SetData( srcBuffer, nativebuf.OffsetInBuffer, 0, outputVectorCount );

            //    Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
            //}

            //return inputDeps;

        }

    }
}
