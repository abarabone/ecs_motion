using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Abarabone.Common
{
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


            this.OnUpdateWithCommandBuffer(cmd);


            // Make sure that the ECB system knows about our job
            this.CommandSystem.AddJobHandleForProducer(this.Dependency);
        }


        protected abstract void OnUpdateWithCommandBuffer(EntityCommandBuffer commandBuffer);

    }
}
