using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;


using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateBefore( typeof( MarkDrawTargetBoneSystem ) )]
    [UpdateBefore( typeof( MarkDrawTargetMotionStreamSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
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
            var nativeInstanceBuffers = this.drawSystem.NativeBuffers;
            if( !nativeInstanceBuffers.Units.IsCreated ) return inputDeps;


            nativeInstanceBuffers.Reset();

            inputDeps = new DrawCullingDummyJob
            {
                NativeBuffers = nativeInstanceBuffers.Units,
            }
            .Schedule( this, inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }


        [BurstCompile]
        struct DrawCullingDummyJob : IJobForEach<DrawModelIndexData, DrawInstanceTargetWorkData>
        {

            [ReadOnly]
            public NativeArray<DrawInstanceNativeBufferUnit> NativeBuffers;


            public void Execute
                ( [ReadOnly] ref DrawModelIndexData model, [WriteOnly] ref DrawInstanceTargetWorkData target )
            {

                target.InstanceIndex = this.NativeBuffers[ model.ModelIndex ].InstanceCounter.GetSerial();
                //target.InstanceIndex = this.InstanceCounters[ 0 ].GetSerial();

            }
        }

    }

}
