using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Cs;
using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;
using Abss.Utilities;

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateAfter(typeof( BoneToDrawInstanceSystem ) )]
    [UpdateInGroup(typeof(DrawSystemGroup))]
    public class BeginDrawCsBarier : EntityCommandBufferSystem
    { }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    //[AlwaysUpdateSystem]
    //[DisableAutoCreation]
    [UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof(DrawSystemGroup))]
    public class DrawMeshCsSystem : JobComponentSystem
    {

        public int MaxInstance = 10000;



        DrawInstanceTempBufferAllocationSystem tempBufferSystem;



        //// 描画用バッファ
        //SimpleComputeBuffer<bone_unit> instanceTransformBuffer;
        //SimpleIndirectArgsBuffer instanceArgumentsBuffer;
        //InstancingIndirectArguments arguments;


        public DrawComputeInstanceBufferHolder ComputeBuffers { get; } = new DrawComputeInstanceBufferHolder();

        public DrawNativeInstanceBufferHolder NativeBuffers { get; } = new DrawNativeInstanceBufferHolder();


        // 描画モデルリソース
        DrawMeshResourceHolder resourceHolder = new DrawMeshResourceHolder();

        public DrawMeshResourceHolder GetResourceHolder() => this.resourceHolder;


        //// ボーンフレームバッファ
        //NativeArray<float4> instanceBoneVectors;
        //NativeArray<NativeSlice<float4>> instanceBoneVectorEveryModels;

        //public NativeArray<NativeSlice<float4>> GetInstanceBoneVectorEveryModels() =>
        //    this.instanceBoneVectorEveryModels;


        //// モデルごとのインスタンスフレームカウンター
        //NativeArray<ThreadSafeCounter<Persistent>> instanceCounters;

        //public NativeArray<ThreadSafeCounter<Persistent>> GetInstanceCounters() =>
        //    this.instanceCounters;



        protected override void OnStartRunning()
        {
            //createBuffers();
            //allocVectors();
            //allocInstanceCounters();

            this.tempBufferSystem = this.World.GetExistingSystem<DrawInstanceTempBufferAllocationSystem>();

            this.ComputeBuffers.Initialize( this.resourceHolder );
            this.NativeBuffers.Initialize( this.resourceHolder );

            return;


            //void createBuffers()
            //{
            //    this.instanceArgumentsBuffer.CreateBuffer();
            //    this.instanceTransformBuffer = new SimpleComputeBuffer<bone_unit>( "bones", 4 * 16 * this.MaxInstance );
            //}

            //void allocVectors()
            //{
            //    var vectorLength = 4;
            //    var instanceMax = this.MaxInstance;
            //    var arrayLengths = this.resourceHolder.Units
            //        .Select( x => vectorLength * x.Mesh.bindposes.Length * instanceMax )
            //        .ToArray();

            //    this.instanceBoneVectorEveryModels =
            //        new NativeArray<NativeSlice<float4>>( arrayLengths.Length, Allocator.Persistent );
                
            //    this.instanceBoneVectors =
            //        new NativeArray<float4>( arrayLengths.Sum(), Allocator.Persistent );
                
            //    var start = 0;
            //    for( var i = 0; i < arrayLengths.Length; i++ )
            //    {
            //        this.instanceBoneVectorEveryModels[ i ] = this.instanceBoneVectors.Slice( start, arrayLengths[ i ] );
            //        start += arrayLengths[ i ];
            //    }
            //}

            //void allocInstanceCounters()
            //{
            //    this.instanceCounters =
            //        new NativeArray<ThreadSafeCounter<Persistent>>( this.resourceHolder.Units.Count, Allocator.Persistent );

            //    for( var i = 0; i < this.instanceCounters.Length; i++ )
            //    {
            //        this.instanceCounters[ i ] = new ThreadSafeCounter<Persistent>( 0 );
            //    }
            //}
        }
        protected override void OnStopRunning()
        {
            //if( this.instanceCounters.IsCreated )
            //{
            //    for( var i = 0; i < this.instanceCounters.Length; i++ )
            //    {
            //        this.instanceCounters[ i ].Dispose();
            //    }
            //    this.instanceCounters.Dispose();
            //}

            //if( this.instanceBoneVectors.IsCreated ) this.instanceBoneVectors.Dispose();
            //if( this.instanceBoneVectorEveryModels.IsCreated ) this.instanceBoneVectorEveryModels.Dispose();

            //this.instanceTransformBuffer.Dispose();
            //this.instanceArgumentsBuffer.Dispose();

            this.resourceHolder.Dispose();
            this.ComputeBuffers.Dispose();
            this.NativeBuffers.Dispose();
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            
            for( var i = 0; i < this.resourceHolder.Units.Count; i++ )
            {
                var resource = this.resourceHolder.Units[ i ];
                var nativebuf = this.NativeBuffers.Units[ i ];
                var devicebuf = this.ComputeBuffers.Units[ i ];

                var mesh = resource.Mesh;
                var mat = resource.Material;
                var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
                var args = devicebuf.InstanceArgumentsBuffer;

                var instanceCount = nativebuf.InstanceCounter.Count;
                var argparams = new IndirectArgumentsForInstancing( mesh, instanceCount );
                args.SetData( ref argparams );

                var boneLength = mesh.bindposes.Length;

                var outputVectorCount = instanceCount * boneLength * nativebuf.VectorLengthOfBone;
                var srcBuffer = this.NativeBuffers.InstanceBoneVectors;//this.tempBufferSystem.TempInstanceBoneVectors;
                var dstBuffer = devicebuf.TransformBuffer;
                dstBuffer.SetData( srcBuffer, nativebuf.OffsetInBuffer, 0, outputVectorCount );
                
                Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
            }

            this.tempBufferSystem.TempInstanceBoneVectors.Dispose();
            // 本体と離れているので忘れないよう…
            
            return inputDeps;
        }

    }
}
