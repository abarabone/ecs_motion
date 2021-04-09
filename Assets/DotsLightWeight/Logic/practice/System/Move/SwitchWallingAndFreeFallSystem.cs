using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;
using Abarabone.Character;
using Abarabone.Geometry;
using Abarabone.Physics;

namespace Abarabone.Character
{
    using Abarabone.Dependency;

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(WallingMoveSystem))]
    [UpdateAfter(typeof(FreeFallWithHitSystem))]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class SwitchWallingAndFreeFallWithHitSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var commands = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            //inputDeps = new SwitchWallingAndFreeFallWithHitJob
            //{
            //    Commands = this.ecb.CreateCommandBuffer().AsParallelWriter(),
            //    WallHitResults = this.GetComponentDataFromEntity<WallHitResultData>( isReadOnly : true ),
            //}
            //.Schedule( this, inputDeps );

            var wallHitResults = this.GetComponentDataFromEntity<WallHitResultData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithAll<WallingTag>()
                .WithReadOnly(wallHitResults)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref WallHangingData walling,
                        ref PhysicsGravityFactor g
                    )
                =>
                    {
                        var isFreeFalling = wallHitResults.HasComponent(entity);

                        if (isFreeFalling)
                        {
                            if (wallHitResults[entity].IsHit)
                            {
                                //commands.RemoveComponent<PhysicsVelocity>(entityInQueryIndex, entity);
                                commands.RemoveComponent<WallHitResultData>(entityInQueryIndex, entity);

                                //commands.AddComponent( jobIndex, entity, new WallHunggingData { } );
                                g = new PhysicsGravityFactor { Value = 0.0f };
                                walling.State = WallHangingData.WallingState.none_rotating;
                            }
                        }

                        if (!isFreeFalling)
                        {
                            if (walling.State > WallHangingData.WallingState.front_45_rotating)
                            {
                                //commands.RemoveComponent<WallHunggingData>( entityInQueryIndex, entity );

                                //commands.AddComponent( jobIndex, entity, new PhysicsVelocity { } );
                                commands.AddComponent(entityInQueryIndex, entity, new WallHitResultData { });
                                g = new PhysicsGravityFactor { Value = 1.0f };
                            }
                        }
                    }
                )
                .ScheduleParallel();
        }


        ////[BurstCompile]
        //[RequireComponentTag(typeof(WallingTag))]
        //struct SwitchWallingAndFreeFallWithHitJob : IJobForEachWithEntity
        //    <WallHunggingData, PhysicsGravityFactor>
        //{

        //    public EntityCommandBuffer.ParallelWriter Commands;

        //    [ReadOnly]
        //    public ComponentDataFromEntity<WallHitResultData> WallHitResults;


        //    public void Execute(
        //        Entity entity, int jobIndex,
        //        ref WallHunggingData walling,
        //        [WriteOnly] ref PhysicsGravityFactor g
        //    )
        //    {

        //        var isFreeFalling = this.WallHitResults.HasComponent( entity );
                

        //        if( isFreeFalling )
        //        {
        //            if( this.WallHitResults[ entity ].IsHit )
        //            {
        //                //this.Commands.RemoveComponent<PhysicsVelocity>( jobIndex, entity );
        //                this.Commands.RemoveComponent<WallHitResultData>( jobIndex, entity );

        //                //this.Commands.AddComponent( jobIndex, entity, new WallHunggingData { } );
        //                g = new PhysicsGravityFactor { Value = 0.0f };
        //                walling.State = 0;
        //            }
        //        }

        //        if( !isFreeFalling )
        //        {
        //            if( walling.State >= 2 )
        //            {
        //                //this.Commands.RemoveComponent<WallHunggingData>( jobIndex, entity );

        //                //this.Commands.AddComponent( jobIndex, entity, new PhysicsVelocity { } );
        //                this.Commands.AddComponent( jobIndex, entity, new WallHitResultData { } );
        //                g = new PhysicsGravityFactor { Value = 1.0f };
        //            }
        //        }

        //    }
        //}
    }
}

