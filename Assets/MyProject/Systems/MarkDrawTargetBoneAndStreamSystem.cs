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

namespace Abss.Draw
{

    public class MarkDrawTargetBoneAndStreamSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            throw new System.NotImplementedException();
        }


        struct MarkBoneJob : IJobForEach<BoneDrawLinkData, >
        {
            [ReadOnly]
            public ComponentDataFromEntity<DrawModelIndexData> DrawIndices;

            public void Execute( ref BoneDrawLinkData bone )
            {

                bone

            }
        }
    }

}
