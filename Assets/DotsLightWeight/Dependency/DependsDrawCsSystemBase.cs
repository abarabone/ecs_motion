using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

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

            this.drawSystem.Waiter.AddDependencyBefore(this.Dependency);
        }


        protected abstract void OnUpdateWith();
    }
}
