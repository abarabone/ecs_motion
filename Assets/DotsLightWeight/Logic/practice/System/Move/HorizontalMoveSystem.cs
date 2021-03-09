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

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;
using Abarabone.Character;

namespace Abarabone.Character
{

    /// <summary>
    /// 与えられた方向を向き、与えられた水平移動をする。
    /// ジャンプが必要なら、地面と接触していればジャンプする。←暫定
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class HorizontalMoveSystem : SystemBase
    {


        //BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            //this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override void OnUpdate()
        {

            //inputDeps = new HorizontalMoveJob
            //{
            //    CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld,//.CollisionWorld,
            //    DeltaTime = this.Time.DeltaTime,//UnityEngine.Time.fixedDeltaTime,//Time.DeltaTime,
            //}
            //.Schedule( this, inputDeps );
            
            //return inputDeps;


            //var collisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld;//.CollisionWorld,
            var deltaTime = this.Time.DeltaTime;//UnityEngine.Time.fixedDeltaTime,//Time.DeltaTime,

            this.Entities
                //.WithReadOnly(collisionWorld)
                //.WithReadOnly(deltaTime)
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref MoveHandlingData handler,
                        ref GroundHitResultData ground,
                        ref Translation pos,
                        ref PhysicsVelocity v
                    ) =>
                    {
                        ref var acts = ref handler.ControlAction;

                        var vlinear = v.Linear;

                        var upf = math.select(0.0f, acts.JumpForce, ground.IsGround);

                        var xzDir = acts.MoveDirection * (deltaTime * 300.0f);
                        acts.JumpForce = 0.0f;//

                        xzDir.y = vlinear.y + upf;

                        v.Linear = xzDir;//math.min( xyDir, new float3( 1000, 1000, 1000 ) );
                    }
                )
                .ScheduleParallel();
        }



        //[BurstCompile, RequireComponentTag(typeof(HorizontalMovingTag))]
        //struct HorizontalMoveJob : IJobForEachWithEntity
        //    <MoveHandlingData, GroundHitResultData, Translation, PhysicsVelocity>
        //{

        //    [ReadOnly] public float DeltaTime;

        //    [ReadOnly] public PhysicsWorld CollisionWorld;


        //    public unsafe void Execute(
        //        Entity entity, int index,
        //        [ReadOnly] ref MoveHandlingData handler,
        //        [ReadOnly] ref GroundHitResultData ground,
        //        [ReadOnly] ref Translation pos,
        //        ref PhysicsVelocity v
        //    )
        //    {
        //        ref var acts = ref handler.ControlAction;

        //        var vlinear = v.Linear;

        //        var upf = math.select( 0.0f, acts.JumpForce, ground.IsGround );

        //        var xzDir = acts.MoveDirection * ( this.DeltaTime * 300.0f );
                
        //        xzDir.y = vlinear.y + upf;

        //        v.Linear = xzDir;//math.min( xyDir, new float3( 1000, 1000, 1000 ) );

        //    }
        //}

    }
}
