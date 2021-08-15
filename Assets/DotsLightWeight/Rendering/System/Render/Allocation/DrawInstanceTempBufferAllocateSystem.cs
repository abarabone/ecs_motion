using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace DotsLite.Draw
{
    using DotsLite.Misc;

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    ////[UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    //////[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    ////[UpdateAfter(typeof(DrawCullingSystem))]
    ////[UpdateAfter(typeof(DrawCullingSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.DrawPrev.TempAlloc))]
    public class DrawInstanceTempBufferAllocateSystem : SystemBase
    {



        EntityQuery drawQuery;



        protected override void OnCreate()
        {
            base.OnCreate();

            this.drawQuery = this.GetEntityQuery(
                ComponentType.ReadOnly<DrawModel.InstanceCounterData>(),
                ComponentType.ReadWrite<DrawModel.VectorIndexData>(),
                ComponentType.ReadOnly<DrawModel.BoneVectorSettingData>()
            );
        }



        protected override unsafe void OnUpdate()
        {
            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>();
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var instanceOffsetType = this.GetComponentTypeHandle<DrawModel.VectorIndexData>();
            var instanceCounterType = this.GetComponentTypeHandle<DrawModel.InstanceCounterData>();// isReadOnly: true );
            var boneInfoType = this.GetComponentTypeHandle<DrawModel.BoneVectorSettingData>();// isReadOnly: true );

            var chunks = this.drawQuery.CreateArchetypeChunkArray( Allocator.TempJob );

            var useTempJobBuffer = this.HasSingleton<DrawSystem.TransformBufferUseTempJobTag>();

            this.Job
                .WithBurst()
                .WithDisposeOnCompletion(chunks)
                .WithNativeDisableParallelForRestriction(nativeBuffers)
                .WithCode(
                    () =>
                    {
                        var totalVectorLength = sumAndSetVectorOffsets_();

                        if (!useTempJobBuffer) return;
                        
                        nativeBuffers[drawSysEnt] = allocateNativeBuffer_(totalVectorLength);
                    }
                )
                .Schedule();

            return;



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
                        var instanceOffset = offsets[j].OptionalVectorLengthPerInstance;

                        offsets[ j ] = new DrawModel.VectorIndexData
                        {
                            ModelStartIndex = sum,
                            OptionalVectorLengthPerInstance = instanceOffset,
                        };

                        var instanceCount = counters[ j ].InstanceCounter.Count;
                        var instanceVectorSize = infos[ j ].BoneLength * infos[ j ].VectorLengthInBone + instanceOffset;
                        var modelBufferSize = instanceCount * instanceVectorSize;

                        sum += modelBufferSize;
                    }
                }

                return sum;
            }

            DrawSystem.NativeTransformBufferData allocateNativeBuffer_(int totalVectorLength)
            {
                var nativeBuffer
                    = new SimpleNativeBuffer<float4>(totalVectorLength, Allocator.TempJob);

                var nativeTransformBuffer
                    = new DrawSystem.NativeTransformBufferData { Transforms = nativeBuffer };

                return nativeTransformBuffer;
            }

        }

    }

}
