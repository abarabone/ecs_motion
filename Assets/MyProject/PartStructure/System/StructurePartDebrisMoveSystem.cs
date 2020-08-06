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


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    //[UpdateAfter(typeof())]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MonolithicBoneTransform.MonolithicBoneTransformSystemGroup))]
    public class StructurePartDebrisMoveSystem : SystemBase
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


            var deltaTime = this.Time.DeltaTime;


            this.Entities
                .WithBurst()

                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref StructurePartDebris.Data debris
                    ) =>
                    {

                        debris.LifeTime -= deltaTime;

                        if( debris.LifeTime <= 0.0f )
                        {
                            cmd.DestroyEntity(entityInQueryIndex, entity);
                        }

                    }
                )
                .ScheduleParallel();

        }

    }

}
