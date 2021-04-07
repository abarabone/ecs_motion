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

    public struct HitMessageSender<THitMessage>
        where THitMessage : struct
    {

        HitMessageRecieverReference<THitMessage> reciever;
        DependencyAccessableSystemBase dependentSystem;


        public HitMessageSender(DependencyAccessableSystemBase senderSystem, HitMessageRecieverReference<THitMessage> recieverReference)
        {
            this.dependentSystem = senderSystem;
            this.reciever = recieverReference;
        }


        public HitMessageRecieverParallelWriter<THitMessage> AsParallelWriter() =>
            this.reciever.writer;


        public DisposableDependency WithDependencyScope() =>
            new DisposableDependency { barrier = this.reciever.barrier, dependentSystem = this.dependentSystem };


        public struct DisposableDependency : IDisposable
        {
            public DependencyBarrier barrier;
            public DependencyAccessableSystemBase dependentSystem;

            public void Dispose()
            {
                var reciever = this.barrier;
                var dep = this.dependentSystem;
                barrier.AddDependencyBefore(dep.GetOutputDependency());
            }
        }
    }

    public partial struct HitMessageReciever<THitMessage, TJobInnerExecution>
    {
        HitMessageRecieverReference<THitMessage> CreateReference() =>
            new HitMessageRecieverReference<THitMessage>
            {
                writer = this.holder.AsParallelWriter(),
                barrier = this.barrier,
            };
    }

    public struct HitMessageRecieverReference<THitMessage>
        where THitMessage : struct
    {
        public HitMessageRecieverParallelWriter<THitMessage> writer;
        public DependencyBarrier barrier;
    }
}
