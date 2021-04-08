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
        BarrierDependencyReciever Barrier { get; }
    }



    //public struct DependencyBarrier : IDisposable
    public class BarrierDependencyReciever : IDisposable
    {



        NativeList<JobHandle> dependencyJobHandles;




        public static BarrierDependencyReciever Create(int capacity = 16) =>
            new BarrierDependencyReciever
            {
                dependencyJobHandles = new NativeList<JobHandle>(capacity, Allocator.Persistent),
            };

        public void Dispose() => this.dependencyJobHandles.Dispose();



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
        

    }


}