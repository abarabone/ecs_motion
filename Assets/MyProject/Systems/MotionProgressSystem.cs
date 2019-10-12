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

namespace Abss.Draw
{
    
    public class MotionProgressSystem : JobComponentSystem
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
            //throw new System.NotImplementedException();

            return inputDeps;
        }



    }

}
