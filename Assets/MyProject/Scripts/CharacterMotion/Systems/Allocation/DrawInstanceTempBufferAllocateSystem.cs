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
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawAllocationGroup ) )]
    //[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    //[UpdateInGroup( typeof( DrawSystemGroup ) )]
    public class DrawInstanceTempBufferAllocateSystem : JobComponentSystem
    {



        EntityQuery drawQuery;




        protected override void OnCreate()
        {
            this.drawQuery = this.GetEntityQuery(
                ComponentType.ReadOnly<DrawModelInstanceCounterData>(),
                ComponentType.ReadWrite<DrawModelInstanceOffsetData>()
            );


        }



        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new DrawInstanceTempBufferAllocateJob
            {
                NativeBuffers = this.GetComponentDataFromEntity<DrawSystemNativeTransformBufferData>(),

                InstanceCounterType = this.GetArchetypeChunkComponentType<DrawModelInstanceCounterData>(isReadOnly:true),
                InstanceOffsetType = this.GetArchetypeChunkComponentType<DrawModelInstanceOffsetData>(),

                BufferLinkType = this.GetArchetypeChunkComponentType<DrawChunkBufferLinkerData>(isReadOnly:true),
            }
            .Schedule( this.drawQuery, inputDeps );


            return inputDeps;
        }




        //[BurstCompile]
        unsafe struct DrawInstanceTempBufferAllocateJob : IJobChunk
        {

            [WriteOnly][NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<DrawSystemNativeTransformBufferData> NativeBuffers; 

            [ReadOnly]
            public ArchetypeChunkComponentType<DrawModelInstanceCounterData> InstanceCounterType;
            public ArchetypeChunkComponentType<DrawModelInstanceOffsetData> InstanceOffsetType;

            [ReadOnly]
            public ArchetypeChunkComponentType<DrawChunkBufferLinkerData> BufferLinkType;


            public void Execute( ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex )
            {

                var counters = chunk.GetNativeArray( this.InstanceCounterType );
                var offsets = chunk.GetNativeArray( this.InstanceOffsetType );

                var sum = 0;
                for( var i=0; i<chunk.Count; i++ )
                {

                    offsets[ i ] = new DrawModelInstanceOffsetData { VectorOffsetInBuffer = sum };

                    sum += counters[ i ].InstanceCounter.Count;

                }

                var ent = chunk.GetChunkComponentData(this.BufferLinkType).BufferEntity;
                this.NativeBuffers[ ent ] = new DrawSystemNativeTransformBufferData
                {
                    Transforms = new SimpleNativeBuffer<float4, Temp>( sum ),
                };

            }
        }
    }

}
