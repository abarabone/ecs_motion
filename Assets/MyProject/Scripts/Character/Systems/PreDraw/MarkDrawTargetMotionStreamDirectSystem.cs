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

    //[DisableAutoCreation]
    //public class MarkDrawTargetMotionStreamDirectSystem : JobComponentSystem
    //{


    //    protected override JobHandle OnUpdate( JobHandle inputDeps )
    //    {
    //        throw new System.NotImplementedException();
    //    }


    //    [BurstCompile]
    //    struct MarkStreamJob : IJobForEach<StreamDrawLinkData, StreamDirectDrawTargetIndexData>
    //    {

    //        [ReadOnly]
    //        public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;


    //        public void Execute(
    //            [ReadOnly] ref StreamDrawLinkData drawLinker,
    //            ref StreamDirectDrawTargetIndexData indexer
    //        )
    //        {

    //            var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

    //            indexer.DrawInstanceVectorIndex = 0;//

    //        }
    //    }

    //}

}
