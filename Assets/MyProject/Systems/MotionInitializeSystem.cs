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
                Shifters = this.GetComponentDataFromEntity<StreamKeyShiftData>(),
                Timers   = this.GetComponentDataFromEntity<StreamTimeProgressData>(),
                Caches   = this.GetComponentDataFromEntity<StreamNearKeysCacheData>(),

            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }



        struct MotionInitializeJob :
            IJobForEachWithEntity_EBCCC<LinkedEntityGroup, MotionInitializeData, MotionClipData, MotionInfoData>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamKeyShiftData>      Shifters;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamNearKeysCacheData> Caches;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<StreamTimeProgressData>  Timers;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] DynamicBuffer<LinkedEntityGroup> links,
                [ReadOnly] ref MotionInitializeData tag,
                [ReadOnly] ref MotionClipData data,
                [ReadOnly] ref MotionInfoData info
            )
            {
                ref var clip = ref data.ClipData.Value;
                ref var motion = ref clip.Motions[info.MotionIndex];

                initSection( ref motion, ref links, KeyStreamSection.positions );
                initSection( ref motion, ref links, KeyStreamSection.rotations );

                this.Commands.RemoveComponent<MotionInitializeData>( index, entity );
            }

            void initSection
                ( ref MotionBlobUnit motion, ref DynamicBuffer<LinkedEntityGroup> links, KeyStreamSection streamSection )
            {
                ref var streams = ref motion.Sections[(int)streamSection].Streams;

                var ioffset = 1 + (int)streamSection * streams.Length;

                for( var i = 0; i < streams.Length; i++ )
                {
                    var streamEntity = links[ i + ioffset ].Value;

                    var shifter = this.Shifters[streamEntity];
                    shifter.Keys = streams[ i ].Keys;
                    this.Shifters[streamEntity] = shifter;

                    var timer = this.Timers[streamEntity];
                    timer.TimeLength    = motion.TimeLength;
                    timer.TimeScale     = 1.0f;
                    timer.TimeProgress  = 0.0f;
                    this.Timers[streamEntity] = timer;

                    var cache = this.Caches[streamEntity];
                    cache.InitializeKeys( ref shifter, ref timer );
                    //timer.Progress( 0.1f );
                    //cache.ShiftKeysIfOverKeyTimeForLooping( ref shifter, ref timer );
                    this.Caches[streamEntity] = cache;
                }
            }

        }

  //      //[BurstCompile]
  //      struct StreamInitializeJob : IJobForEachWithEntity
  //          <StreamInitialTag, StreamKeyShiftData, StreamNearKeysCacheData, StreamTimeProgressData>
		//{

		//	public EntityCommandBuffer.Concurrent Commands;

  //          public ComponentDataFromEntity<MotionInfoData> MotionInfos;
            

  //          public void Execute(
  //              Entity entity, int index,
  //              [ReadOnly] ref StreamInitialTag tag,
  //              ref StreamKeyShiftData shifter,
  //              ref StreamNearKeysCacheData cache,
  //              ref StreamTimeProgressData timer
  //          )
  //          {
  //              var ma = this.MotionInfos[tag.MotionEntity].DataAccessor;

  //              timer.TimeProgress  = 0.0f;
  //              timer.TimeScale     = 1.0f;
  //              timer.TimeLength    = ma.TimeLength;

  //              //shifter.Keys = ma.GetStreamSlice( i >> 2, KeyStreamSection.positions + ( i & 1 ) ).Keys;

  //              cache.InitializeKeys( ref shifter, ref timer );

		//		Commands.RemoveComponent<StreamInitialTag>( index, entity );
  //          }
  //      }

    }

}

