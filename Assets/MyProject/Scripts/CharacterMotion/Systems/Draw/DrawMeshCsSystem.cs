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
    public class DrawMeshCsSystem : ComponentSystem
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


        protected override unsafe void OnUpdate()
        {

            var nativeBuffer = this.GetSingleton<DrawSystemNativeTransformBufferData>().Transforms;
            var computeBuffer = this.GetSingleton<DrawSystemComputeTransformBufferData>().Transforms;//
            
            var na = NativeArrayUnsafeUtility
                .ConvertExistingDataToNativeArray<float4>( nativeBuffer.pBuffer, nativeBuffer.length_, Allocator.None );

            computeBuffer.SetData( na );


            this.Entities
                .ForEach(
                    (
                        ref DrawModelBoneUnitSizeData boneUnitInfo,
                        ref DrawModelInstanceCounterData counter
                        //in DrawModelComputeArgumentsBufferData shaderArg,
                        //ref DrawModelGeometryData geom
                    ) =>
                    {

                        //var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };

                        ////var args = shaderArg.InstanceArgumentsBuffer;
                        ////var instanceCount = instanceCounter.InstanceCounter.Count;
                        ////var argparams = new IndirectArgumentsForInstancing( geom.Mesh, instanceCount );
                        ////args.SetData( ref argparams );
                        
                        //var mesh = geom.Mesh;


                        //var mat = geom.Material;
                        ////mat.SetBuffer( "BoneVectorBuffer", dstBuffer );
                        ////mat.SetInt( "BoneVectorLength", mesh.bindposes.Length * (int)boneType );
                        //mat.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要

                        ////Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
                    }
                );
            


            return;

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
