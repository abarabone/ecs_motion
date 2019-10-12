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

using Abss.Cs;
using Abss.Arthuring;
using Abss.Motion;

namespace Abss.Draw
{

    public class BoneToDrawInstanceSystem : JobComponentSystem
    {


        //EntityQuery query;


        protected override void OnCreate()
        {
            var query = this.GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new con
                }
            );
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //throw new System.NotImplementedException();

            return inputDeps;
        }


        struct BoneToDrawInstanceJob : IJobForEach<Translation,Rotation>
        {

        }

    }

}
