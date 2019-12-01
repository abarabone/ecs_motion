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



        [BurstCompile, RequireComponentTag(typeof(WallHunggingData))]
        struct HorizontalMoveJob : IJobForEachWithEntity
            <Translation, Rotation, PhysicsVelocity>
        {

            [ReadOnly] public float DeltaTime;

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot,
                ref PhysicsVelocity v
            )
            {

                var dir = math.forward( rot.Value );
                var move = dir * (this.DeltaTime * 170.0f);
                var rtf = new RigidTransform( rot.Value, pos.Value );

                var localCenter = new float3( 0.0f, 1.5f, 0.0f );//

                var a = this.CollisionWorld.Bodies;
                //var st = math.transform( rtf, localCenter );
                //var ed = st + math.mul( rot.Value, ray.DirectionAndLength.As_float3() ) * ray.DirectionAndLength.w;
                //var hitInput = new RaycastInput
                //{
                //    Start = st,
                //    End = ed,
                //    Filter = ray.filter,
                //};
                //var isHit = this.CollisionWorld.CastRay( hitInput, );// 自身のコライダを除外できればシンプルになるんだが…


                var y = v.Linear.y;
                v.Linear = move;
                v.Linear.y = y;

            }
        }


    }
}
