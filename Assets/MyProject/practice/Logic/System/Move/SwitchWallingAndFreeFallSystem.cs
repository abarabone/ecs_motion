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

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(WallingMoveSystem))]
    [UpdateAfter( typeof( FreeFallWithHitSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
    //[UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class SwitchWallingAndFreeFallWithHitSystem : JobComponentSystem
    {


        EntityCommandBufferSystem ecb;



        protected override void OnCreate()
        {
            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
            //this.ecb = this.World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //return inputDeps;
            inputDeps = new SwitchWallingAndFreeFallWithHitJob
            {
                Commands = this.ecb.CreateCommandBuffer().AsParallelWriter(),
                WallHitResults = this.GetComponentDataFromEntity<WallHitResultData>( isReadOnly : true ),
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }


        //[BurstCompile]
        [RequireComponentTag(typeof(WallingTag))]
        struct SwitchWallingAndFreeFallWithHitJob : IJobForEachWithEntity
            <WallHunggingData, PhysicsGravityFactor>
        {

            public EntityCommandBuffer.ParallelWriter Commands;

            [ReadOnly]
            public ComponentDataFromEntity<WallHitResultData> WallHitResults;


            public void Execute(
                Entity entity, int jobIndex,
                ref WallHunggingData walling,
                [WriteOnly] ref PhysicsGravityFactor g
            )
            {

                var isFreeFalling = this.WallHitResults.HasComponent( entity );
                

                if( isFreeFalling )
                {
                    if( this.WallHitResults[ entity ].IsHit )
                    {
                        //this.Commands.RemoveComponent<PhysicsVelocity>( jobIndex, entity );
                        this.Commands.RemoveComponent<WallHitResultData>( jobIndex, entity );

                        //this.Commands.AddComponent( jobIndex, entity, new WallHunggingData { } );
                        g = new PhysicsGravityFactor { Value = 0.0f };
                        walling.State = 0;
                    }
                }

                if( !isFreeFalling )
                {
                    if( walling.State >= 2 )
                    {
                        //this.Commands.RemoveComponent<WallHunggingData>( jobIndex, entity );

                        //this.Commands.AddComponent( jobIndex, entity, new PhysicsVelocity { } );
                        this.Commands.AddComponent( jobIndex, entity, new WallHitResultData { } );
                        g = new PhysicsGravityFactor { Value = 1.0f };
                    }
                }

            }
        }
    }
}

