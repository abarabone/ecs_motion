using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateBefore( typeof( DrawCullingDummySystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class DrawInstanceCounterResetSystem : ComponentSystem
    {

        protected override void OnStartRunning()
        {
            this.Entities
                .ForEach(
                    ( ref DrawModelInstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter = new ThreadSafeCounter<Persistent>(0);
                    }
                );
        }

        protected override void OnUpdate()
        {

            this.Entities
                .ForEach(
                    ( ref DrawModelInstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter.Reset();
                    }
                );

        }

        protected override void OnDestroy()
        {
            this.Entities
                .ForEach(
                    ( ref DrawModelInstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter.Dispose();
                    }
                );
        }

    }

}
