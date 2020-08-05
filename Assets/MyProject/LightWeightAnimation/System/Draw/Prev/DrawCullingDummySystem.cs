using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace Abarabone.Draw
{
    using Abarabone.Authoring;
    using Abarabone.Misc;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    

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
            this.c = Camera.main;//
        }

        Camera c;//
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var drawModels = this.GetComponentDataFromEntity<DrawModel.InstanceCounterData>();

            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var scls = this.GetComponentDataFromEntity<NonUniformScale>(isReadOnly: true);


            var cam = this.c;// Camera.main;
            var viewFrustum = new ViewFrustumSoa(cam);


            inputDeps = this.Entities
                .WithBurst( FloatMode.Fast, FloatPrecision.Standard )
                .WithNativeDisableParallelForRestriction( drawModels )
                .ForEach(
                        (
                            ref DrawInstance.TargetWorkData target,
                            in DrawInstance.ModeLinkData linker
                        ) =>
                    {

                        var rot = rots[linker.];

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
