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
        //public NativeArray<float4> TempInstanceBoneVectors { get; private set; }
        public JobAllocatableBuffer<float4, Temp> TempInstanceBoneVectors { get; private set; }

        DrawMeshCsSystem drawMeshCsSystem;
        
        public JobHandle inputDeps { get; private set; }//


        protected override void OnStartRunning()
        {
            this.drawMeshCsSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            if( !this.drawMeshCsSystem.NativeBuffers.Units.IsCreated )
                return inputDeps;

            this.TempInstanceBoneVectors = new JobAllocatableBuffer<float4, Temp>( 0 );
            // .Dispose() は、DrawMeshCsSystem にて行う。離れているので注意。

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
            //this.TempInstanceBoneVectors.Dispose();
            // DrawMeshCsSystem で適切に行われれば、必要ない
        }



        //[BurstCompile]
        unsafe struct DrawInstanceTempBufferAllocationJob : IJob
        {

            [ReadOnly]
            public NativeArray<DrawInstanceNativeBufferUnit> NativeInstances;

            public JobAllocatableBuffer<float4, Temp> InstanceVectorBuffer;


            public unsafe void Execute()
            {
                var length = 0;
                foreach( var x in this.NativeInstances )
                {
                    length += x.InstanceCounter.Count;
                }

                this.InstanceVectorBuffer.AllocateBuffer( length );
            }
        }
    }


    public unsafe struct JobAllocatableBuffer<T,Tallocator> : System.IDisposable
        where T:struct
        where Tallocator:IAllocatorLabel,new()
    {

        [NativeDisableContainerSafetyRestriction]
        NativeArray<int> bufferPointerHolder;

        public void* pBuffer
        {
            get => (void*)this.bufferPointerHolder[ 0 ];
            private set => this.bufferPointerHolder[ 0 ] = (int)value;
        }


        public JobAllocatableBuffer( int length )
        {
            var allocator = new Tallocator().Label;
            this.bufferPointerHolder = new NativeArray<int>( 1, allocator, NativeArrayOptions.ClearMemory );

            if( length > 0 ) this.AllocateBuffer( length );
        }

        public void AllocateBuffer( int length )
        {
            var size = UnsafeUtility.SizeOf<T>();
            var align = UnsafeUtility.AlignOf<T>();
            var allocator = new Tallocator().Label;

            this.pBuffer = UnsafeUtility.Malloc( size * length, align, allocator );
        }

        public void Dispose()
        {
            if( !this.bufferPointerHolder.IsCreated ) return;
            
            if( this.pBuffer != null )
                disposeBuffer_( this.pBuffer );

            this.bufferPointerHolder.Dispose();


            void disposeBuffer_( void* pBuffer )
            {
                var allocator = new Tallocator().Label;
                UnsafeUtility.Free( (void*)pBuffer, allocator );
            }
        }

    }
}
