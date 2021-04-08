using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System;

namespace Abarabone.Dependency
{

    public struct BarrierDependencySender
    {

        IBarrierable barrierSystem;
        DependencyAccessableSystemBase dependentSystem;


        public static BarrierDependencySender Create<TBarrierableSystem>(DependencyAccessableSystemBase system)
            where TBarrierableSystem : SystemBase, IBarrierable
        =>
            new BarrierDependencySender
            {
                barrierSystem = system.World.GetExistingSystem<TBarrierableSystem>(),
                dependentSystem = system,
            };


        public DependencyScope WithDependencyScope()
        {
            //dependentSystem.AddInputDependency(this.barrierSystem.GetOutputDependency());

            return new DependencyScope { parent = this };
        }


        public struct DependencyScope : IDisposable
        {
            public BarrierDependencySender parent;

            public void Dispose()
            {
                var bar = this.parent.barrierSystem.Barrier;
                var dep = this.parent.dependentSystem;
                bar.AddDependencyBefore(dep.GetOutputDependency());
            }
        }
    }

}


