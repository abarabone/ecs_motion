using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

using Abss.Cs;
using Abss.Arthuring;
using Abss.Motion;
using Abss.SystemGroup;
using Abss.Misc;

namespace Abss.Draw
{
    /// <summary>
    /// ジョブで依存関係をスケジュールしてバッファを確保したいが、うまくいかない
    /// そもそもジョブで確保できるのか、外部に渡せるのかもわからない
    /// </summary>
    //[DisableAutoCreation]
    [AlwaysUpdateSystem]
    [UpdateInGroup( typeof( DrawAllocationGroup ) )]
    //[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    //[UpdateInGroup( typeof( DrawSystemGroup ) )]
    public class DrawInstanceTempBufferAllocationSystem : JobComponentSystem
    {



        // 一括ボーンフレームバッファ
        public NativeArray<float4> TempInstanceBoneVectors { get; private set; }

        DrawMeshCsSystem drawMeshCsSystem;


        public JobHandle inputDeps;//


        protected override void OnStartRunning()
        {
            this.drawMeshCsSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
            //this.TempInstanceBoneVectors = new NativeArray<float4>( 1, Allocator.Temp );//
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            if( !this.drawMeshCsSystem.NativeBuffers.Units.IsCreated )
                return inputDeps;

            //if( this.TempInstanceBoneVectors.IsCreated )
            //    this.TempInstanceBoneVectors.Dispose();

            this.TempInstanceBoneVectors = new NativeArray<float4>( 1, Allocator.TempJob );//
            
            inputDeps = new DrawInstanceTempBufferAllocationJob
            {
                NativeInstances = this.drawMeshCsSystem.NativeBuffers.Units,
                InstanceVectorBuffer = this.TempInstanceBoneVectors,
            }
            .Schedule( inputDeps );
            this.inputDeps = inputDeps;//
            
            return inputDeps;
        }

        protected override void OnDestroy()
        {
            if( this.TempInstanceBoneVectors.IsCreated )
                this.TempInstanceBoneVectors.Dispose();
        }



        //[BurstCompile]
        unsafe struct DrawInstanceTempBufferAllocationJob : IJob
        {

            [ReadOnly]
            public NativeArray<DrawInstanceNativeBufferUnit> NativeInstances;

            //[NativeDisableParallelForRestriction]
            //public NativeArray<float4> InstanceVectorBuffer;
            public void *pSystem;


            public unsafe void Execute()
            {

                var length = 0;
                foreach( var x in this.NativeInstances )
                {
                    length += x.InstanceCounter.Count;
                }

                var p = (DrawInstanceTempBufferAllocationSystem*)
                //this.InstanceVectorBuffer.Dispose();
                //this.InstanceVectorBuffer = new NativeArray<float4>( length, Allocator.Temp );
            }
        }
    }

    public unsafe struct JobAllocatableBuffer<T> : System.IDisposable
        where T:struct
    {
        NativeArray<int> bufferPointerHolder;

        public JobAllocatableBuffer( Allocator allocator ) =>
            this.bufferPointerHolder = new NativeArray<int>( 1, allocator );

        public void Dispose() =>
            this.bufferPointerHolder.Dispose();

        public void AllocBuffer( int length, Allocator allocator )
        {
            var size = UnsafeUtility.SizeOf<T>();
            var align = UnsafeUtility.AlignOf<T>();
            this.bufferPointerHolder[ 0 ] = (int)UnsafeUtility.Malloc( size, align, allocator );
        }
        public void DisposeBuffer()
        {
            UnsafeUtility.Free( this.bufferPointerHolder[0], allo
        }
    }
}
