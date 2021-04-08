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
    using Abarabone.Dependency;


        /// <summary>
        /// 現在は剛体に任せているが、いずれはここで計算したい
        /// （現在は生存管理だけ）
        /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    //[UpdateAfter(typeof())]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MonolithicBoneTransform.MonolithicBoneTransformSystemGroup))]
    public class StructurePartDebrisMoveSystem : DependencyAccessableSystemBase
    {


        CommandBufferDependencySender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependencySender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

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
