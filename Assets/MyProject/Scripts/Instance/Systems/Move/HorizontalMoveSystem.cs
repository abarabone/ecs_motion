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
using Abss.SystemGroup;
using Abss.Instance;

namespace Abss.Character
{

    /// <summary>
    /// 与えられた方向を向き、与えられた水平移動をする。
    /// ジャンプが必要なら、地面と接触していればジャンプする。←暫定
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class HorizontalMoveSystem : JobComponentSystem
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
            <MoveHandlingData, Translation, PhysicsVelocity>
        {

            [ReadOnly] public float DeltaTime;

            [ReadOnly] public CollisionWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [ReadOnly] ref MoveHandlingData handler,
                [ReadOnly] ref Translation pos,
                ref PhysicsVelocity v
            )
            {

                ref var acts = ref handler.ControlAction;


                var upf = 0.0f;

                if( acts.JumpForce > 0.0f )
                {
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

                    var a = new NativeList<DistanceHit>( Allocator.Temp );
                    var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );
                    if( isHit && a.Length > 1 )// 自身のコライダを除外できればシンプルになるんだが…
                    {
                        upf = acts.JumpForce * 0.5f;
                    }
                    a.Dispose();
                }

                var vlinear = v.Linear;
                var xyDir = acts.MoveDirection * this.DeltaTime * 170;

                xyDir.y = vlinear.y + upf;

                v.Linear = math.min( xyDir, new float3( 10, 1000, 10 ) );

            }
        }

    }
}
