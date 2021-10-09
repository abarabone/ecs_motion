using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System;

namespace DotsLite.Dependency
{
    public static partial class BarrierDependency
    {

        public struct Sender
        {

            IRecievable barrierSystem;
            DependencyAccessableSystemBase dependentSystem;


            public static BarrierDependency.Sender Create<TBarrierableSystem>(DependencyAccessableSystemBase system)
                where TBarrierableSystem : SystemBase, IRecievable
            =>
                new Sender
                {
                    //barrierSystem = system.World.GetExistingSystem<TBarrierableSystem>(),
                    barrierSystem = system.World.GetOrCreateSystem<TBarrierableSystem>(),
                    dependentSystem = system,
                };


            public DependencyScope WithDependencyScope()
            {
                //dependentSystem.AddInputDependency(this.barrierSystem.GetOutputDependency());

                return new DependencyScope { parent = this };
            }


            public struct DependencyScope : IDisposable
            {
                public Sender parent { set; private get; }

                public void Dispose()
                {
                    var bar = this.parent.barrierSystem.Reciever;
                    var dep = this.parent.dependentSystem;
                    bar.AddDependencyBefore(dep.GetOutputDependency());
                }
            }
        }

    }
}


