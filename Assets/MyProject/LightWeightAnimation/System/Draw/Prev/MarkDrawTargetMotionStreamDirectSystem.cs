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
using Abarabone.CharacterMotion;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

    //[DisableAutoCreation]
    //public class MarkDrawTargetMotionStreamDirectSystem : JobComponentSystem
    //{


    //    protected override JobHandle OnUpdate( JobHandle inputDeps )
    //    {
    //        throw new System.NotImplementedException();
    //    }


    //    [BurstCompile]
    //    struct MarkStreamJob : IJobForEach<Stream.DrawLinkData, Stream.DirectDrawTargetIndexData>
    //    {

    //        [ReadOnly]
    //        public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;


    //        public void Execute(
    //            [ReadOnly] ref Stream.DrawLinkData drawLinker,
    //            ref Stream.DirectDrawTargetIndexData indexer
    //        )
    //        {

    //            var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

    //            indexer.DrawInstanceVectorIndex = 0;//

    //        }
    //    }

    //}

}
