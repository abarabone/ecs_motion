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
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

using Abss.Misc;
using Abss.Utilities;
using Abss.Instance;
using Abss.SystemGroup;

namespace Abss.Character
{

    [DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    public class IsGroundAroundSystem : JobComponentSystem
    {
        



        protected override void OnCreate()
        {

        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            return inputDeps;
        }




        [BurstCompile]
        struct IsGroundJob : IJobForEachWithEntity
            <GroundHitResultData, GroundHitSphereData, Translation>
        {

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [WriteOnly] ref GroundHitResultData ground,
                [ReadOnly] ref GroundHitSphereData sphere,
                [ReadOnly] ref Translation pos
            )
            {
                var hitInput = new PointDistanceInput
                {
                    Position = pos.Value + sphere.Center,
                    MaxDistance = sphere.Distance,
                    Filter = sphere.filter,
                };
                
                var a = new NativeList<DistanceHit>( Allocator.Temp );
                var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );
                if( isHit && a.Length > 1 )// 自身のコライダを除外できればシンプルになるんだが…
                {
                    ground = new GroundHitResultData { IsGround = true };
                }
                a.Dispose();
            }
        }

    }
}
