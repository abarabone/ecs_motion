﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;


using Abarabone.Authoring;
using Abarabone.Motion;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    [UpdateAfter( typeof( DrawCullingDummySystem ) )]
    public class MarkDrawTargetMotionStreamSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new MarkStreamJob
            {
                DrawInstanceIndexers = this.GetComponentDataFromEntity<DrawInstance.TargetWorkData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            
            return inputDeps;
        }


        [BurstCompile]
        struct MarkStreamJob : IJobForEach<StreamDrawLinkData, StreamDrawTargetData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawInstance.TargetWorkData> DrawInstanceIndexers;


            public void Execute(
                [ReadOnly] ref StreamDrawLinkData drawLinker,
                ref StreamDrawTargetData targetFlag
            )
            {

                var drawIndexer = this.DrawInstanceIndexers[ drawLinker.DrawEntity ];

                targetFlag.IsDrawTarget = drawIndexer.DrawInstanceId > -1;

            }
        }

    }

}
