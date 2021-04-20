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
            //var collisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld;//.CollisionWorld,
            var deltaTime = this.Time.DeltaTime;//UnityEngine.Time.fixedDeltaTime,//Time.DeltaTime,

            this.Entities
                .WithBurst()
                //.WithReadOnly(collisionWorld)
                //.WithReadOnly(deltaTime)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        //ref MoveHandlingData handler,
                        ref GroundHitResultData ground,
                        ref Translation pos,
                        ref PhysicsVelocity v,
                        in Control.MoveData moves
                    ) =>
                    {
                        //ref var acts = ref handler.ControlAction;

                        var vlinear = v.Linear.xz;
                        var dir = moves.MoveDirection.xz;

                        var d = dir * (deltaTime * 300.0f);
                        v.Linear += new float3(d - vlinear, 0.0f).xzy * 0.9f;

                        //var d = dir * (deltaTime * 300.0f);
                        //var l = math.length(vlinear + d);
                        //var limit = 20.0f;
                        //var limited = limit - math.min(l, limit);
                        //var n = (vlinear + d) * math.select(math.rcp(l), 0.0f, l == 0.0f);

                        //var upf = math.select(0.0f, acts.JumpForce, ground.IsGround);
                        //xzDir.y = vlinear.y + upf;
                        //acts.JumpForce = 0.0f;//

                        //v.Linear += new float3(limited * n - vlinear, 0.0f).xzy;
                    }
                )
                .ScheduleParallel();
        }


    }
}
