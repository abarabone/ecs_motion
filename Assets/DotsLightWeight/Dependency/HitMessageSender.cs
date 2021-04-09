using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.LowLevel;

namespace Abarabone.Dependency
{


    public static partial class HitMessage<THitMessage>
    {


        public struct Sender
        {

            IRecievable recievableSystem;
            DependencyAccessableSystemBase dependentSystem;



            public static Sender Create<TRecievable>(DependencyAccessableSystemBase senderSystem)
                where TRecievable : SystemBase, IRecievable
            =>
                new Sender
                {
                    dependentSystem = senderSystem,
                    recievableSystem = senderSystem.World.GetExistingSystem<TRecievable>(),
                };


            public ParallelWriter AsParallelWriter() =>
                this.recievableSystem.Reciever.Holder.AsParallelWriter();


            public DisposableDependency WithDependencyScope() =>
                new DisposableDependency { parent = this };


            public struct DisposableDependency : IDisposable
            {
                public Sender parent;

                public void Dispose()
                {
                    var barrier = this.parent.recievableSystem.Reciever.Barrier;
                    var dep = this.parent.dependentSystem;
                    barrier.AddDependencyBefore(dep.GetOutputDependency());
                }
            }
        }

    }

}
