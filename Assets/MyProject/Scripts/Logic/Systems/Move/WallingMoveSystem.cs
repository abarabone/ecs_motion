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

namespace Abss.Character
{

    /// <summary>
    /// 与えられた方向を向き、与えられた水平移動をする。
    /// ジャンプが必要なら、地面と接触していればジャンプする。←暫定
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class WallingMoveSystem : JobComponentSystem
    {


        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            inputDeps = new HorizontalMoveJob
            {
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
                DeltaTime = Time.deltaTime,
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile]
        struct HorizontalMoveJob : IJobForEachWithEntity
            <WallHunggingData, MoveHandlingData, GroundHitSphereData, Translation, Rotation, PhysicsVelocity>
        {

            [ReadOnly] public float DeltaTime;

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [ReadOnly] ref WallHunggingData walling,
                [ReadOnly] ref MoveHandlingData handler,
                [ReadOnly] ref GroundHitSphereData sphere,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot,
                ref PhysicsVelocity v
            )
            {

                var rtf = new RigidTransform( rot.Value, pos.Value );

                var dir = math.forward( rot.Value );
                var move = dir * (this.DeltaTime * 170.0f);
                
                
                var st = math.transform( rtf, ray.Start ) + dir * ray.Ray.Length;
                var ed = st + math.mul( rot.Value, ray.Ray.Line );
                var hitInput = new RaycastInput
                {
                    Start = st,
                    End = ed,
                    Filter = ray.Filter,
                };
                var collector = new AnyRayHitExcludeSelfCollector( 1.0f, entity, this.CollisionWorld.Bodies );
                var isHit = this.CollisionWorld.CastRay( hitInput, ref collector );

                if( collector.NumHits > 0 ) move = collector.

                var y = v.Linear.y;
                v.Linear = move;
                v.Linear.y = y;

            }
        }


    }
}
