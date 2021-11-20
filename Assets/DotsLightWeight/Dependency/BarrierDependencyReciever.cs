using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace DotsLite.Dependency
{
    public static partial class BarrierDependency
    {

        /// <summary>
        /// 
        /// </summary>
        public interface IRecievable
        {
            BarrierDependency.Reciever Reciever { get; }
        }

    }

    public static partial class BarrierDependency
    {


        //public struct DependencyBarrier : IDisposable
        public class Reciever : IDisposable
        {



            NativeList<JobHandle> dependencyJobHandles;




            public static Reciever Create(int capacity = 16) =>
                new Reciever
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

                return CombineAllDependentJobs();
            }
            public JobHandle CombineAllDependentJobs()
            {
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

            public void CompleteAllDependentJobs()
            {
                this.CombineAllDependentJobs().Complete();
            }

        }

    }
}