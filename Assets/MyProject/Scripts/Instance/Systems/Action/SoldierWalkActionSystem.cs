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

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;

namespace Abss.Character
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class SoldierWalkActionSystem : JobComponentSystem
    {


        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないとエラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            return inputDeps;
        }



        [BurstCompile]
        struct SoldierMoveJob : IJobForEachWithEntity
            <Translation, PhysicsVelocity>
        {

            [ReadOnly] public float3 StickDir;
            [ReadOnly] public quaternion CamRotWorld;
            [ReadOnly] public float DeltaTime;
            [ReadOnly] public float JumpForce;

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                //[ReadOnly] ref GroundHitColliderData hit,
                [ReadOnly] ref Translation pos,
                ref PhysicsVelocity v
            )
            {

                var upf = 0.0f;

                if( this.JumpForce > 0.0f )
                {
                    //var hitInput = new ColliderCastInput
                    //{
                    //    Collider = (Collider*)hit.Collider.GetUnsafePtr(),
                    //    Orientation = quaternion.identity,
                    //    Start = pos.Value + math.up() * 0.05f,
                    //    End = pos.Value + math.up() * -0.1f,
                    //};
                    var hitInput = new PointDistanceInput
                    {
                        Position = pos.Value,
                        MaxDistance = 0.1f,
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ( 1 << 20 ) | ( 1 << 22 ) | ( 1 << 23 ),
                            CollidesWith = ( 1 << 20 ) | ( 1 << 22 ) | ( 1 << 23 ),
                            GroupIndex = 0,
                        },
                    };
                    //var collector = new ExcludeEntityCollector
                    //{
                    //    IgnoreEntity = entity,
                    //    Rigidbodies = this.CollisionWorld.Bodies,
                    //};
                    var a = new NativeList<DistanceHit>( Allocator.Temp );
                    var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );
                    if( isHit && a.Length > 1 )
                    {
                        upf = this.JumpForce * 0.5f;
                    }
                    a.Dispose();
                }

                var vlinear = v.Linear;
                var xyDir = math.rotate( this.CamRotWorld, this.StickDir ) * this.DeltaTime * 170;

                xyDir.y = vlinear.y + upf;

                v.Linear = math.min( xyDir, new float3( 10, 1000, 10 ) );

            }
        }

    }
}
