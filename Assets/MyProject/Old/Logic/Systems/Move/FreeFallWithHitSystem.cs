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
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
    //[UpdateInGroup( typeof( ObjectMoveSystemGroup ) )]
    public class FreeFallWithHitSystem : JobComponentSystem
    {


        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //return inputDeps;
            inputDeps = new FreeFallWithHitJob
            {
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld,//.CollisionWorld,
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }


        [BurstCompile]
        struct FreeFallWithHitJob : IJobForEachWithEntity
            <WallHitResultData, GroundHitSphereData, Translation, Rotation>
        {

            [ReadOnly] public PhysicsWorld CollisionWorld;
            //public PhysicsWorld PhysicsWorld;


            public void Execute(
                Entity entity, int jobIndex,
                [WriteOnly] ref WallHitResultData result,
                [ReadOnly] ref GroundHitSphereData sphere,
                [WriteOnly] ref Translation pos,
                [WriteOnly] ref Rotation rot
            )
            {

                //var rtf = new RigidTransform( rot.Value, pos.Value );

                var hitInput = new PointDistanceInput
                {
                    Position = pos.Value,//math.transform( rtf, sphere.Center ),
                    MaxDistance = sphere.Distance,
                    Filter = sphere.Filter,
                };
                //var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );// 自身のコライダを除外できればシンプルになるんだが…

                var collector = new ClosestHitExcludeSelfCollector<DistanceHit>( sphere.Distance, entity );
                //var collector = new ClosestHitCollector<DistanceHit>( sphere.Distance );
                var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref collector );

                if( collector.NumHits == 0 ) return;


                result.IsHit = true;

                var n = collector.ClosestHit.SurfaceNormal;
                var p = collector.ClosestHit.Position;
                pos.Value = p + n * sphere.Distance;

                var right = math.mul( rot.Value, new float3( 1.0f, 0.0f, 0.0f ) );
                var forward = math.cross( n, right );
                var safe_forward = math.select( math.forward(rot.Value), forward, math.dot(right, n) > 0.001f );
                rot.Value = quaternion.LookRotation( safe_forward, n );
                
            }
        }
    }
}
