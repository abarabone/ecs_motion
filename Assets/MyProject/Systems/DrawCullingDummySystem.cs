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
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateBefore( typeof( MarkDrawTargetBoneSystem ) )]
    [UpdateBefore( typeof( MarkDrawTargetStreamSystem ) )]
    [UpdateInGroup(typeof(DrawPrevSystemGroup))]
    public class DrawCullingDummySystem : JobComponentSystem
    {

        DrawMeshCsSystem drawSystem;
        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証


        protected override void OnStartRunning()
        {
            this.drawSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var instanceCounters = this.drawSystem.GetInstanceCounters();
            if( !instanceCounters.IsCreated ) return inputDeps;


            inputDeps = new DrawCullingDummyJob
            {
                InstanceCounters = instanceCounters,
            }
            .Schedule( this, inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }


        //[BurstCompile]
        struct DrawCullingDummyJob : IJobForEach<DrawModelIndexData>
        {

            public NativeArray<ThreadSafeCounter<Persistent>> InstanceCounters;


            public void Execute( ref DrawModelIndexData model )
            {

                model.instanceIndex = this.InstanceCounters[model.modelIndex].GetSerial();

            }
        }

    }

}
