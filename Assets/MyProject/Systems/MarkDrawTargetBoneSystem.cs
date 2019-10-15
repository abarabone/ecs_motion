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
using Abss.Motion;
using Abss.SystemGroup;

namespace Abss.Draw
{
    
    [UpdateInGroup(typeof( DrawPrevSystemGroup ) )]
    public class MarkDrawTargetBoneSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new MarkBoneJob
            {
                DrawIndexers = this.GetComponentDataFromEntity<DrawModelIndexData>( isReadOnly: true ),
                DrawTargets = this.GetComponentDataFromEntity<DrawInstanceTargetWorkData>( isReadOnly:true ),
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }


        [BurstCompile]
        struct MarkBoneJob : IJobForEach<BoneDrawLinkData, BoneIdData, BoneDrawTargetIndexWorkData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;
            [ReadOnly]
            public ComponentDataFromEntity<DrawInstanceTargetWorkData> DrawTargets;

            public void Execute(
                [ReadOnly] ref BoneDrawLinkData drawLinker,
                [ReadOnly] ref BoneIdData boneId,
                ref BoneDrawTargetIndexWorkData boneIndexer
            )
            {

                var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];
                var drawTarget = this.DrawTargets[ drawLinker.DrawEntity ];

                boneIndexer.InstanceBoneOffset =
                    drawTarget.InstanceIndex * drawIndexer.BoneLength + boneId.BoneId;

            }
        }
        
    }

}
