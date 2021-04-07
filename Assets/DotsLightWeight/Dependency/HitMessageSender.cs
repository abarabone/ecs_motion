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

        public struct Sender<TRecievable>
            where TRecievable : SystemBase, IRecievable<THitMessage>
        {

            IRecievable<THitMessage> recievable;
            DependencyAccessableSystemBase dependentSystem;


            public Sender(DependencyAccessableSystemBase senderSystem)
            {
                this.dependentSystem = senderSystem;
                this.recievable = senderSystem.World.GetExistingSystem<TRecievable>();
            }


            public ParallelWriter AsParallelWriter() =>
                this.recievable.Reciever.Holder.AsParallelWriter();


            public DisposableDependency WithDependencyScope() =>
                new DisposableDependency { parent = this };

            public struct DisposableDependency : IDisposable
            {
                public Sender<TRecievable> parent;

                public void Dispose()
                {
                    var barrier = this.parent.recievable.Reciever.Barrier;
                    var dep = this.parent.dependentSystem;
                    barrier.AddDependencyBefore(dep.GetOutputDependency());
                }
            }
        }

    }

}
