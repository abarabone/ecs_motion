using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System;

namespace Abarabone.Dependency
{

    public struct RegisterBarrier<TBarrierableSystem>
        where TBarrierableSystem : SystemBase, IBarrierable
    {

        IBarrierable barrierSystem;
        DependencyAccessableSystemBase dependentSystem;


        public RegisterBarrier(DependencyAccessableSystemBase system)
        {
            this.barrierSystem = system.World.GetExistingSystem<TBarrierableSystem>();
            this.dependentSystem = system;
        }


        public DisposableDependency WithDependencyScope()
        {
            //dependentSystem.AddInputDependency(this.barrierSystem.GetOutputDependency());

            return new DisposableDependency { parent = this };
        }


        public struct DisposableDependency : IDisposable
        {
            public RegisterBarrier<TBarrierableSystem> parent;

            public void Dispose()
            {
                var bar = this.parent.barrierSystem.Barrier;
                var dep = this.parent.dependentSystem;
                bar.AddDependencyBefore(dep.GetOutputDependency());
            }
        }
    }

}



namespace Abarabone.Draw
{



    public abstract class DependsDrawCsSystemBase : SystemBase
    {

        protected DrawMeshCsSystem drawSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.drawSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
        }

        protected override void OnUpdate()
        {

            this.OnUpdateWith();

            this.drawSystem.Barrier.AddDependencyBefore(this.Dependency);
        }


        protected abstract void OnUpdateWith();
    }
}
