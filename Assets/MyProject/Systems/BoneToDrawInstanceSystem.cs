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

    public class BoneToDrawInstanceSystem : JobComponentSystem
    {



        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            throw new System.NotImplementedException();
        }

    }

}
