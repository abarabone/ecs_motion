using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Abarabone.Common
{



    public abstract class DependentableSystemBase : SystemBase
    {
        public JobHandle GetOutputDependency() => this.Dependency;

        public void AddInputDependency(JobHandle dependency) =>
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, dependency);
    }





    struct DependencyWaiter : IDisposable
    {

        NativeList<JobHandle> dependencyJobHandles;


        public DependencyWaiter(int capacity)
        {
            this.dependencyJobHandles = new NativeList<JobHandle>(capacity, Allocator.Persistent);
        }


        public void AddDependencyBeforeHitApply(JobHandle jobHandle) => this.dependencyJobHandles.Add(jobHandle);


        public void WaitAllDependencyJobs()
        {
            JobHandle.CombineDependencies(this.dependencyJobHandles).Complete();

            this.dependencyJobHandles.Clear();
        }


        public void Dispose() => this.dependencyJobHandles.Dispose();
    }


}