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
            return inputDeps;
        }


        struct MarkBoneJob : IJobForEach<BoneDrawLinkData, BoneDrawTargetIndexData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;


            public void Execute(
                [ReadOnly] ref BoneDrawLinkData drawLinker,
                ref BoneDrawTargetIndexData indexer
            )
            {

                var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

                indexer.InstanceBoneOffset = drawIndexer.BoneLength * drawIndexer.instanceIndex;

            }
        }
        
    }

    [UpdateInGroup( typeof( DrawPrevSystemGroup ) )]
    public class MarkDrawTargetStreamSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            return inputDeps;
        }


        struct MarkStreamJob : IJobForEach<StreamDrawLinkData, StreamDrawTargetData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;


            public void Execute(
                [ReadOnly] ref StreamDrawLinkData drawLinker,
                ref StreamDrawTargetData targetFlag
            )
            {

                var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

                targetFlag.IsDrawTarget = drawIndexer.instanceIndex > -1;

            }
        }

    }

    [DisableAutoCreation]
    public class MarkDrawTargetStreamDirectSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            throw new System.NotImplementedException();
        }


        struct MarkStreamJob : IJobForEach<StreamDrawLinkData, StreamDirectDrawTargetIndexData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;


            public void Execute(
                [ReadOnly] ref StreamDrawLinkData drawLinker,
                ref StreamDirectDrawTargetIndexData indexer
            )
            {

                var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

                indexer.DrawInstanceVectorIndex = 0;//

            }
        }

    }

}
