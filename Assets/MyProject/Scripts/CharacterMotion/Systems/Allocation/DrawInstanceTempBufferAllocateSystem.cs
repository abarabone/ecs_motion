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
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    //[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    [UpdateAfter(typeof(DrawCullingDummySystem))]
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
                BoneInfoType = this.GetArchetypeChunkComponentType<DrawModelBoneUnitSizeData>( isReadOnly: true ),

                BufferLinkType = this.GetArchetypeChunkComponentType<DrawModelBufferLinkerData>(isReadOnly:true),
            }
            .Schedule( this.drawQuery, inputDeps );


            //inputDeps = this.Job
            //    .WithCode(
            //        () =>
            //        {

            //        }
            //    )
            //    .Schedule( inputDeps );

            return inputDeps;
        }






        //[BurstCompile]
        unsafe struct DrawInstanceTempBufferAllocateJob : IJob
        {

            [WriteOnly][NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<DrawSystemNativeTransformBufferData> NativeBuffers;


            [ReadOnly][DeallocateOnJobCompletion]
            public NativeArray<ArchetypeChunk> Chunks;


            public ArchetypeChunkComponentType<DrawModelInstanceOffsetData> InstanceOffsetType;
            [ReadOnly]
            public ArchetypeChunkComponentType<DrawModelInstanceCounterData> InstanceCounterType;
            [ReadOnly]
            public ArchetypeChunkComponentType<DrawModelBoneUnitSizeData> BoneInfoType;

            [ReadOnly]
            public ArchetypeChunkComponentType<DrawModelBufferLinkerData> BufferLinkType;


            public void Execute()
            {






            }

            void Chunk_( ArchetypeChunk chunk )
            {
                var counters = chunk.GetNativeArray( this.InstanceCounterType );
                var offsets = chunk.GetNativeArray( this.InstanceOffsetType );
                var infos = chunk.GetNativeArray( this.BoneInfoType );

                var vectorOffsets = stackalloc int[ chunk.Count ];


                var sum = 0;
                for( var i = 0; i < chunk.Count; i++ )
                {
                    vectorOffsets[ i ] = sum;

                    var instanceCount = counters[ i ].InstanceCounter.Count;
                    var instanceVectorSize = infos[ i ].BoneLength * infos[ i ].VectorLengthInBone;
                    var modelBufferSize = instanceCount * instanceVectorSize;
                    sum += modelBufferSize;
                }
            }
            void allocateNativeBuffer( ArchetypeChunk chunk )
            {
                var ent = chunk.GetChunkComponentData( this.BufferLinkType ).BufferEntity;
                this.NativeBuffers[ ent ] = new DrawSystemNativeTransformBufferData
                {
                    Transforms = new SimpleNativeBuffer<float4, Temp>( sum ),
                };
            }
            void setBufferStartPositionForModel( ArchetypeChunk chunk )
            {

            

                var pBufferStart = this.NativeBuffers[ ent ].Transforms.pBuffer;
                    for(var i = 0; i<chunk.Count; i++ )
                    {
                        offsets[ i ] = new DrawModelInstanceOffsetData
                        {
                            pVectorOffsetInBuffer = pBufferStart + vectorOffsets[ i ],
                        };
    }
}
        }
    }

}
