using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Cs;
using Abss.Arthuring;
using Abss.Misc;

namespace Abss.Draw
{

    public class DrawCullingDummySystem : JobComponentSystem
    {

        ThreadSafeCounter<Persistent> instanceIndexSeed;


        protected override void OnCreate()
        {
            this.instanceIndexSeed = new Misc.ThreadSafeCounter<Misc.Persistent>( 0 );
        }
        protected override void OnDestroy()
        {
            this.instanceIndexSeed.Dispose();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //this.instanceIndexSeed.Reset();

            inputDeps = new DrawCullingDummyJob
            {
                InstanceIndexSeed = this.instanceIndexSeed,
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }


        struct DrawCullingDummyJob : IJobForEach<DrawModelIndexData>
        {
            public ThreadSafeCounter<Persistent> InstanceIndexSeed;

            public void Execute( ref DrawModelIndexData model )
            {

                model.instanceIndex = this.InstanceIndexSeed.GetSerial();

            }
        }

    }

}
