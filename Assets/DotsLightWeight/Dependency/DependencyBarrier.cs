using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Abarabone.Dependency
{

    
    public interface IBarrierable
    {
        DependencyBarrier Barrier { get; }
    }



    public struct DependencyBarrier : IDisposable
    {



        NativeList<JobHandle> dependencyJobHandles;




        public DependencyBarrier(int capacity)
        {
            this.dependencyJobHandles = new NativeList<JobHandle>(capacity, Allocator.Persistent);
        }



        /// <summary>
        /// 
        /// </summary>
        public void AddDependencyBefore(JobHandle jobHandle) => this.dependencyJobHandles.Add(jobHandle);



        /// <summary>
        /// 
        /// </summary>
        public JobHandle CombineAllDependentJobs(JobHandle prevDependency)
        {
            this.dependencyJobHandles.Add(prevDependency);

            var resultJob = JobHandle.CombineDependencies(this.dependencyJobHandles);

            this.dependencyJobHandles.Clear();

            return resultJob;
        }



        /// <summary>
        /// 
        /// </summary>
        public void CompleteAllDependentJobs(JobHandle prevDependency)
        {
            this.CombineAllDependentJobs(prevDependency).Complete();
        }
        


        public void Dispose() => this.dependencyJobHandles.Dispose();
    }


}