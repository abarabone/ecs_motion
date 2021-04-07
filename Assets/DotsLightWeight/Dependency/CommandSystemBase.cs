using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Abarabone.Dependency
{

    public struct CommandBufferDependency<TEntityCommandBufferSystem>
        where TEntityCommandBufferSystem : EntityCommandBufferSystem
    {

        EntityCommandBufferSystem commandSystem;
        DependencyAccessableSystemBase dependentSystem;


        public CommandBufferDependency(DependencyAccessableSystemBase system)
        {
            this.commandSystem = system.World.GetExistingSystem<TEntityCommandBufferSystem>();
            this.dependentSystem = system;
        }


        public EntityCommandBuffer CreateCommandBuffer() =>
            this.commandSystem.CreateCommandBuffer();


        public DisposableDependency WithDependencyScope() =>
            new DisposableDependency { parent = this };


        public struct DisposableDependency : IDisposable
        {
            public CommandBufferDependency<TEntityCommandBufferSystem> parent;

            public void Dispose()
            {
                var cmd = this.parent.commandSystem;
                var dep = this.parent.dependentSystem.GetOutputDependency();
                cmd.AddJobHandleForProducer(dep);
            }
        }
    }


    public abstract class CommandSystemBase<TEntityCommandBufferSystem> : SystemBase
        where TEntityCommandBufferSystem : EntityCommandBufferSystem
    {

        protected EntityCommandBufferSystem CommandSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.CommandSystem = this.World.GetExistingSystem<TEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var cmd = this.CommandSystem.CreateCommandBuffer();


            this.OnUpdateWith(cmd);


            // Make sure that the ECB system knows about our job
            this.CommandSystem.AddJobHandleForProducer(this.Dependency);
        }


        protected abstract void OnUpdateWith(EntityCommandBuffer commandBuffer);

    }
}

