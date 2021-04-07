using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

namespace Abarabone.Dependency
{

    public abstract class DependencyAccessableSystemBase : SystemBase
    {


        public JobHandle GetOutputDependency() => this.Dependency;



        public void AddInputDependency(JobHandle dependency) =>
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, dependency);


    }


}