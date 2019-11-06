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
            <MotionInitializeData, MotionStreamLinkData, MotionClipData, MotionInfoData>
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
                [ReadOnly] ref MotionInitializeData init,
                [ReadOnly] ref MotionStreamLinkData linker,
                [ReadOnly] ref MotionClipData data,
                ref MotionInfoData info
            )
            {
                ref var clip = ref data.ClipData.Value;
                ref var motion = ref clip.Motions[ init.MotionIndex];

                info.MotionIndex = init.MotionIndex;
                initSection( ref motion, linker.PositionStreamTop, KeyStreamSection.positions );
                initSection( ref motion, linker.RotationStreamTop, KeyStreamSection.rotations );

                this.Commands.RemoveComponent<MotionInitializeData>( index, entity );
            }

            unsafe void initSection
                ( ref MotionBlobUnit motion, Entity entTop, KeyStreamSection streamSection )
            {
                ref var streams = ref motion.Sections[(int)streamSection].Streams;

                for( var ent = entTop; ent != Entity.Null; ent = this.Linkers[ent].NextStreamEntity )
                {
                    var i = Linkers[ ent ].BoneId;
                    
                    var shifter = this.Shifters[ ent ];
                    var prevKeyPtr = shifter.Keys;// 仮
                    shifter.Keys = (KeyBlobUnit*)streams[ i ].Keys.GetUnsafePtr();
                    shifter.KeyLength = streams[ i ].Keys.Length;

                    var timer = this.Timers[ ent ];
                    timer.TimeLength    = motion.TimeLength;
                    timer.TimeScale     = 1.0f;
                    //timer.TimeProgress = 0.0f;

                    if( prevKeyPtr != null )// 仮
                    {
                        var cache_ = this.Caches[ ent ];
                        cache_.InitializeKeysContinuous( ref shifter, ref timer );
                        this.Caches[ ent ] = cache_;
                        this.Timers[ ent ] = timer;
                        this.Shifters[ ent ] = shifter;
                        continue;
                    }

                    var cache = this.Caches[ ent ];
                    cache.InitializeKeys( ref shifter, ref timer );

                    this.Caches[ ent ] = cache;
                    this.Timers[ ent ] = timer;
                    this.Shifters[ ent ] = shifter;
                }
            }

        }

    }

}

