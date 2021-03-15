using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Abarabone.Draw
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Structure;
    using System.Runtime.CompilerServices;


    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class StructureSleepSwitchingSystem : SystemBase
    {

        EntityCommandBufferSystem cmdSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();

            var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);
            //var excludes = this.GetComponentDataFromEntity<PhysicsExclude>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);
            var disableds = this.GetComponentDataFromEntity<Disabled>(isReadOnly: true);


            this.Entities
                .WithBurst()

                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex
                    )
                =>
                    {

                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}
