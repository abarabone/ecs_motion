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
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using Unity.Collections.LowLevel.Unsafe;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateAfter(typeof( DrawMeshCsSystem ) )]
    [UpdateInGroup( typeof( DrawSystemGroup ) )]
    public class DrawResourceHoderSystem : ComponentSystem
    {



        // 一括ボーンフレームバッファ
        //public NativeArray<float4> TempInstanceBoneVectors { get; private set; }
        public JobAllocatableBuffer<float4, Temp> TempInstanceBoneVectors { get; private set; }

        DrawMeshCsSystem drawMeshCsSystem;

        public JobHandle inputDeps { get; private set; }//



        public int MaxInstance = 10000;



        DrawInstanceTempBufferAllocateSystem tempBufferSystem;




        public DrawComputeInstanceBufferHolder ComputeBuffers { get; } = new DrawComputeInstanceBufferHolder();

        public DrawNativeInstanceBufferHolder NativeBuffers { get; } = new DrawNativeInstanceBufferHolder();


        // 描画モデルリソース
        DrawMeshResourceHolder resourceHolder = new DrawMeshResourceHolder();

        public DrawMeshResourceHolder GetResourceHolder() => this.resourceHolder;




        protected override void OnStartRunning()
        {

            this.tempBufferSystem = this.World.GetExistingSystem<DrawInstanceTempBufferAllocateSystem>();

            this.ComputeBuffers.Initialize( this.resourceHolder );
            this.NativeBuffers.Initialize( this.resourceHolder );

        }

        protected override void OnStopRunning()
        {
            this.resourceHolder.Dispose();
            this.ComputeBuffers.Dispose();
            this.NativeBuffers.Dispose();
        }




        protected override void OnCreate()
        {

        }
        



        public



        protected override void OnCreate()
        {
            this.Enabled = false;
        }



        protected override void OnUpdate()
        { }

    }

    public unsafe struct JobAllocatableBuffer<T, Tallocator> : System.IDisposable
        where T : struct
        where Tallocator : IAllocatorLabel, new()
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
