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
using Abarabone.Misc;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

    [UpdateBefore( typeof( DrawCullingDummySystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class DrawInstanceCounterResetSystem : ComponentSystem
    {

        protected override void OnStartRunning()
        {
            this.Entities
                .ForEach(
                    ( ref DrawModel.InstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter = new ThreadSafeCounter<Persistent>(0);
                    }
                );
        }

        protected override void OnUpdate()
        {

            this.Entities
                .ForEach(
                    ( ref DrawModel.InstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter.Reset();
                    }
                );

        }

        protected override void OnDestroy()
        {
            this.Entities
                .ForEach(
                    ( ref DrawModel.InstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter.Dispose();
                    }
                );
        }

    }

}
