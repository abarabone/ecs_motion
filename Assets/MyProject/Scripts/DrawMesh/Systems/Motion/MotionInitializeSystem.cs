using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.SystemGroup;

namespace Abss.Motion
{
    
    [UpdateBefore(typeof(MotionProgressSystem))]
    [UpdateInGroup(typeof(MotionSystemGroup))]
    public class MotionInitializeSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;


        protected override void OnCreate()
        {
            this.ecb = World.Active.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var commandBuffer = this.ecb.CreateCommandBuffer();


            inputDeps = new MotionInitializeJob
            {
                Commands = commandBuffer.ToConcurrent(),
                Linkers  = this.GetComponentDataFromEntity<StreamRelationData>(),
                Shifters = this.GetComponentDataFromEntity<StreamKeyShiftData>(),
                Timers   = this.GetComponentDataFromEntity<StreamTimeProgressData>(),
                Caches   = this.GetComponentDataFromEntity<StreamNearKeysCacheData>(),

            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }



        //[BurstCompile]
        struct MotionInitializeJob : IJobForEachWithEntity
            <MotionStreamLinkData, MotionInitializeTag, MotionClipData, MotionInfoData>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [NativeDisableParallelForRestriction][ReadOnly]
            public ComponentDataFromEntity<StreamRelationData>      Linkers;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamKeyShiftData>      Shifters;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamNearKeysCacheData> Caches;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamTimeProgressData>  Timers;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref MotionStreamLinkData linker,
                [ReadOnly] ref MotionInitializeTag tag,
                [ReadOnly] ref MotionClipData data,
                [ReadOnly] ref MotionInfoData info
            )
            {
                ref var clip = ref data.ClipData.Value;
                ref var motion = ref clip.Motions[info.MotionIndex];

                initSection( ref motion, linker.PositionStreamTop, KeyStreamSection.positions );
                initSection( ref motion, linker.RotationStreamTop, KeyStreamSection.rotations );

                this.Commands.RemoveComponent<MotionInitializeTag>( index, entity );
            }

            unsafe void initSection
                ( ref MotionBlobUnit motion, Entity entTop, KeyStreamSection streamSection )
            {
                ref var streams = ref motion.Sections[(int)streamSection].Streams;

                for( var ent = entTop; ent != Entity.Null; ent = this.Linkers[ent].NextStreamEntity )
                {
                    var i = Linkers[ ent ].BoneId;
                    
                    var shifter = this.Shifters[ ent ];
                    shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
                    shifter.KeyLength = streams[ i ].Keys.Length;
                    this.Shifters[ ent ] = shifter;

                    var timer = this.Timers[ ent ];
                    timer.TimeLength    = motion.TimeLength;
                    timer.TimeScale     = 1.0f;
                    timer.TimeProgress  = 0.0f;
                    this.Timers[ ent ] = timer;

                    var cache = this.Caches[ ent ];
                    cache.InitializeKeys( ref shifter, ref timer );
                    this.Caches[ ent ] = cache;
                }
            }

        }

    }

}

