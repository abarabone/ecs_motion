using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

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
            //var commandBuffer = this.ecb.CreateCommandBuffer();


            //inputDeps = new StreamInitializeJob
            //{
            //    Commands = commandBuffer.ToConcurrent()
            //}
            //.Schedule( this, inputDeps );

            //this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }



        //struct MotionInitializeJob : IJobForEachWithEntity<MotionInitializeData, MotionInfoData>
        //{

        //    public ComponentDataFromEntity<StreamKeyShiftData>      Shifters;
        //    public ComponentDataFromEntity<StreamTimeProgressData>  Timers;

        //    public void Execute(
        //        Entity entity, int index,
        //        [ReadOnly] ref MotionInitializeData tag,
        //        [ReadOnly] ref MotionInfoData info 
        //    )
        //    {

        //    }
        //}

        //[BurstCompile]
        struct StreamInitializeJob : IJobForEachWithEntity
            <StreamInitialTag, StreamKeyShiftData, StreamNearKeysCacheData, StreamTimeProgressData>
		{

			public EntityCommandBuffer.Concurrent Commands;

            public ComponentDataFromEntity<MotionInfoData> MotionInfos;
            

            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref StreamInitialTag tag,
                ref StreamKeyShiftData shifter,
                ref StreamNearKeysCacheData cache,
                ref StreamTimeProgressData timer
            )
            {
                var ma = this.MotionInfos[tag.MotionEntity].DataAccessor;

                timer.TimeProgress  = 0.0f;
                timer.TimeScale     = 1.0f;
                timer.TimeLength    = ma.TimeLength;

                //shifter.Keys = ma.GetStreamSlice( i >> 2, KeyStreamSection.positions + ( i & 1 ) ).Keys;

                cache.InitializeKeys( ref shifter, ref timer );

				Commands.RemoveComponent<StreamInitialTag>( index, entity );
            }
        }

    }

}

