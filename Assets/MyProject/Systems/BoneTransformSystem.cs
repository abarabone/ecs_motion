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
using Abss.SystemGroup;

namespace Abss.Motion
{

    //[DisableAutoCreation]
    [UpdateAfter(typeof(MotionProgressSystem))]
    [UpdateInGroup(typeof(MotionSystemGroup))]
    public class BoneTransformSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {

        }

        protected override void OnDestroy()
        {

        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            return inputDeps;
        }


        //struct BoneTransformJobLv0 : IJobForEach<BoneLevel0Data, BoneStreamLinkData, BonePostureData>
        //{

        //    [ReadOnly]
        //    public ComponentDataFromEntity<BonePostureData> ParentBones;

        //    [ReadOnly]
        //    public ComponentDataFromEntity<StreamInterpolatedData> Streams;


        //    public void Execute(
        //        [ReadOnly] ref BoneLevel0Data parentLinker, 
        //        [ReadOnly] ref BoneStreamLinkData streamLinker,
        //        [WriteOnly] ref BonePostureData posture
        //    )
        //    {



        //    }
        //}
    }

}
