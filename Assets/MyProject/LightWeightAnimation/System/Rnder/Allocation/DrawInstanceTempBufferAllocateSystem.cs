using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Abarabone.Draw
{
    using Abarabone.Misc;

    /// <summary>
    /// ジョブで依存関係をスケジュールしてバッファを確保したいが、うまくいかない
    /// そもそもジョブで確保できるのか、外部に渡せるのかもわからない
    /// </summary>
    //[DisableAutoCreation]
    ////[UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    //////[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    ////[UpdateAfter(typeof(DrawCullingSystem))]
    ////[UpdateAfter(typeof(DrawCullingSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.TempAlloc))]
    public class DrawInstanceTempBufferAllocateSystem : JobComponentSystem
    {



        EntityQuery drawQuery;



        protected override void OnCreate()
        {
            this.drawQuery = this.GetEntityQuery(
                ComponentType.ReadOnly<DrawModel.InstanceCounterData>(),
                ComponentType.ReadWrite<DrawModel.InstanceOffsetData>(),
                ComponentType.ReadOnly<DrawModel.BoneVectorSettingData>()
            );
        }



        protected override unsafe JobHandle OnUpdate( JobHandle inputDeps )
        {
            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>();
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var instanceOffsetType = this.GetComponentTypeHandle<DrawModel.InstanceOffsetData>();
            var instanceCounterType = this.GetComponentTypeHandle<DrawModel.InstanceCounterData>();// isReadOnly: true );
            var boneInfoType = this.GetComponentTypeHandle<DrawModel.BoneVectorSettingData>();// isReadOnly: true );

            var chunks = this.drawQuery.CreateArchetypeChunkArray( Allocator.TempJob );

            inputDeps = this.Job
                .WithBurst()
                .WithDeallocateOnJobCompletion( chunks )
                .WithNativeDisableParallelForRestriction( nativeBuffers )
                .WithCode(
                    () =>
                    {
                        var totalVectorLength = sumAndSetVectorOffsets_();

                        var nativeBuffer
                            = new SimpleNativeBuffer<float4, TempJob>( totalVectorLength );

                        nativeBuffers[ drawSysEnt ]
                            = new DrawSystem.NativeTransformBufferData { Transforms = nativeBuffer };

                        calculateVectorOffsetPointersInBuffer_( nativeBuffer.pBuffer );
                    }
                )
                .Schedule( inputDeps );

            return inputDeps;



            int sumAndSetVectorOffsets_()
            {
                var sum = 0;
                for( var i = 0; i < chunks.Length; i++ )
                {
                    var chunk = chunks[ i ];
                    var offsets = chunk.GetNativeArray( instanceOffsetType );
                    var counters = chunk.GetNativeArray( instanceCounterType );
                    var infos = chunk.GetNativeArray( boneInfoType );

                    for( var j = 0; j < chunk.Count; j++ )
                    {
                        var instanceOffset = offsets[j].VectorOffsetPerInstance;

                        offsets[ j ] = new DrawModel.InstanceOffsetData
                        {
                            VectorOffsetPerModel = sum,
                            VectorOffsetPerInstance = instanceOffset,
                        };

                        var instanceCount = counters[ j ].InstanceCounter.Count;
                        var instanceVectorSize = infos[ j ].BoneLength * infos[ j ].VectorLengthInBone + instanceOffset;
                        var modelBufferSize = instanceCount * instanceVectorSize;

                        sum += modelBufferSize;
                    }
                }

                return sum;
            }

            void calculateVectorOffsetPointersInBuffer_( float4* pBufferStart )
            {
                for( var i = 0; i < chunks.Length; i++ )
                {
                    var chunk = chunks[ i ];
                    var offsets = chunk.GetNativeArray( instanceOffsetType );

                    for( var j = 0; j < chunk.Count; j++ )
                    {
                        var offset = offsets[ j ];

                        offset.pVectorOffsetPerModelInBuffer = pBufferStart + offset.VectorOffsetPerModel;

                        offsets[j] = offset;
                    }
                }
            }

        }

    }

}
