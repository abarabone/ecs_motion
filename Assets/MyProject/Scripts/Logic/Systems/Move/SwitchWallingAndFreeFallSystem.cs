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

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;
using Abss.Character;
using Abss.Geometry;
using Abss.Physics;

namespace Abss.Character
{

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SimulationSystemGroup ) )]
    //[UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class SwitchWallingAndFreeFallWithHitSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //return inputDeps;
            inputDeps = new SwitchWallingAndFreeFallWithHitJob
            {
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }


        //[BurstCompile]
        struct SwitchWallingAndFreeFallWithHitJob : IJobForEachWithEntity
            <WallHunggingData, WallHitResultData, GroundHitSphereData, PhysicsGravityFactor>
        {



            public void Execute(
                Entity entity, int jobIndex,
                [ReadOnly] ref WallHunggingData walling,
                [ReadOnly] ref WallHitResultData result,
                [ReadOnly] ref GroundHitSphereData sphere,
                [WriteOnly] ref PhysicsGravityFactor g
            )
            {

                if( this.Wallings.Exists( post ) )
                {
                    if( this.Wallings[ post ].State >= 2 )
                    {
                        this.Commands.RemoveComponent<WallHunggingData>( jobIndex, post );

                        //this.Commands.AddComponent( jobIndex, post, new PhysicsVelocity { } );
                        this.Commands.AddComponent( jobIndex, post, new WallHitResultData { } );
                        this.GravityFactors[ post ] = new PhysicsGravityFactor { Value = 1.0f };
                    }
                }

                if( this.WallHitResults.Exists( post ) )
                {
                    if( this.WallHitResults[ post ].IsHit )
                    {
                        //this.Commands.RemoveComponent<PhysicsVelocity>( jobIndex, post );
                        this.Commands.RemoveComponent<WallHitResultData>( jobIndex, post );

                        this.Commands.AddComponent( jobIndex, post, new WallHunggingData { } );
                        this.GravityFactors[ post ] = new PhysicsGravityFactor { Value = 0.0f };
                    }
                }

            }
        }
    }
}

