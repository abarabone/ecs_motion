using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;


using Abarabone.Authoring;
using Abarabone.Misc;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

    [UpdateAfter( typeof( DrawInstanceCounterResetSystem ) )]
    [UpdateBefore( typeof( MarkDrawTargetBoneSystem ) )]
    [UpdateBefore( typeof( MarkDrawTargetMotionStreamSystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class DrawCullingDummySystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証


        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var drawModels = this.GetComponentDataFromEntity<DrawModelInstanceCounterData>();


            inputDeps = this.Entities
                .WithBurst( FloatMode.Fast, FloatPrecision.Standard )
                .WithNativeDisableParallelForRestriction( drawModels )
                .ForEach(
                    ( ref DrawInstanceTargetWorkData target, in DrawInstanceModeLinkData linker ) =>
                    {

                        var drawModelData = drawModels[ linker.DrawModelEntity ];

                        target.DrawInstanceId = drawModelData.InstanceCounter.GetSerial();

                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }

    }

}
