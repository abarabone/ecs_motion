﻿using System.Collections;
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
using Abss.Motion;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateInGroup( typeof( DrawPrevSystemGroup ) )]
    public class MarkDrawTargetMotionStreamSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new MarkStreamJob
            {
                DrawInstanceIndexers = this.GetComponentDataFromEntity<DrawInstanceTargetWorkData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            
            return inputDeps;
        }


        [BurstCompile]
        struct MarkStreamJob : IJobForEach<StreamDrawLinkData, StreamDrawTargetData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawInstanceTargetWorkData> DrawInstanceIndexers;


            public void Execute(
                [ReadOnly] ref StreamDrawLinkData drawLinker,
                ref StreamDrawTargetData targetFlag
            )
            {

                var drawIndexer = this.DrawInstanceIndexers[ drawLinker.DrawEntity ];

                targetFlag.IsDrawTarget = drawIndexer.InstanceIndex > -1;

            }
        }

    }

}
