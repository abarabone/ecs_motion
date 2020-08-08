using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Entities.UniversalDelegates;
using Unity.Physics.Systems;
using System;

namespace Abarabone.Misc
{

    using Random = Unity.Mathematics.Random;


    [DisableAutoCreation]
    public class RandomConcurrentSystem : SystemBase
    {


        protected override void OnCreate()
        {
            base.OnCreate();

        }


        protected override void OnUpdate()
        {
            //var rnd = Random.CreateFromIndex();
        }

    }

}