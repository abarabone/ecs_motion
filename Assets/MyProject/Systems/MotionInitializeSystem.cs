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


            inputDeps = new StreamInitializeJob
            {
                Commands = commandBuffer.ToConcurrent()
            }
            .Schedule( this, inputDeps );

            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }


        
        //struct MotionInitializeJob : IJobForEach<MotionInitializeData>
        //{

        //}

		//[BurstCompile]
		struct StreamInitializeJob : IJobForEachWithEntity<StreamInitialTag, StreamKeyShiftData, StreamNearKeysCacheData>
		{

			public EntityCommandBuffer.Concurrent	Commands;
            

            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref StreamInitialTag tag,
                ref StreamKeyShiftData shifter,
                ref StreamNearKeysCacheData cache
            )
            {

                StreamExtentions.InitializeKeys( ref nearKeys, ref shiftInfo );


				Commands.RemoveComponent<StreamInitialTag>( index, entity );
            }
        }

    }

}

