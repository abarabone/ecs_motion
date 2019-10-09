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

namespace Abss.Motion
{

    public class BoneTransformSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            throw new System.NotImplementedException();
        }


        struct BoneTransformJobLv0 : IJobForEach<BoneLevel0Data, BoneStreamLinkData, BonePostureData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<BonePostureData> ParentBones;

            [ReadOnly]
            public ComponentDataFromEntity<StreamInterpolatedData> Streams;


            public void Execute(
                [ReadOnly] ref BoneLevel0Data parentLinker, 
                [ReadOnly] ref BoneStreamLinkData streamLinker,
                [WriteOnly] ref BonePostureData posture
            )
            {



            }
        }
    }

}
