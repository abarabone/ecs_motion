using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace DotsLite.Dependency
{

    public abstract partial class DependencyAccessableSystemBase : SystemBase
    {


        public JobHandle GetOutputDependency() => this.Dependency;



        public void AddInputDependency(JobHandle dependency) =>
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, dependency);


    }


}