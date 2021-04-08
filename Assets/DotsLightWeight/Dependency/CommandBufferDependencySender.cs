using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Abarabone.Dependency
{

    public struct CommandBufferDependencySender
    {

        EntityCommandBufferSystem commandSystem;
        DependencyAccessableSystemBase dependentSystem;


        public static CommandBufferDependencySender Create<TEntityCommandBufferSystem>(DependencyAccessableSystemBase system)
            where TEntityCommandBufferSystem : EntityCommandBufferSystem
        =>
            new CommandBufferDependencySender
            {
                commandSystem = system.World.GetExistingSystem<TEntityCommandBufferSystem>(),
                dependentSystem = system,
            };
        


        public EntityCommandBuffer CreateCommandBuffer() =>
            this.commandSystem.CreateCommandBuffer();


        public DisposableDependency WithDependencyScope() =>
            new DisposableDependency { parent = this };


        public struct DisposableDependency : IDisposable
        {
            public CommandBufferDependencySender parent;

            public void Dispose()
            {
                var cmd = this.parent.commandSystem;
                var dep = this.parent.dependentSystem.GetOutputDependency();
                cmd.AddJobHandleForProducer(dep);
            }
        }
    }

}

