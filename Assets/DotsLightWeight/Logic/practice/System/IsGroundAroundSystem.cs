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
using UnityEngine.Assertions;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Abarabone.Character
{

    using Abarabone.CharacterMotion;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Character;
    using Abarabone.Physics;
    using Abarabone.SystemGroup;
    using Abarabone.Model;


    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
    public class IsGroundAroundSystem : SystemBase
    {
        
        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }


        protected override void OnUpdate()
        {

            var cw = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;

            var mainEntities = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithReadOnly(mainEntities)
                .WithReadOnly(cw)
                .ForEach(
                    (
                        Entity entity,
                        ref GroundHitResultData ground,
                        in GroundHitSphereData sphere,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        var rtf = new RigidTransform(rot.Value, pos.Value);

                        var hitInput = new PointDistanceInput
                        {
                            Position = math.transform(rtf, sphere.Center),
                            MaxDistance = sphere.Distance,
                            Filter = sphere.Filter,
                        };
                        var collector = new AnyHitExcludeSelfCollector<DistanceHit>(sphere.Distance, entity, mainEntities);
                        var isHit = cw.CalculateDistance(hitInput, ref collector);

                        ground.IsGround = collector.NumHits > 0;
                    }
                )
                .ScheduleParallel();


            //var mainEntities = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);

            //inputDeps = new IsGroundAroundJob
            //{
            //    CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld,//.CollisionWorld,
            //    MainEntities = mainEntities,
            //}
            //.Schedule( this, inputDeps );


            //return inputDeps;
        }




        [BurstCompile]
        struct IsGroundAroundJob : IJobForEachWithEntity
            <GroundHitResultData, GroundHitSphereData, Translation, Rotation>
        {

            [ReadOnly] public PhysicsWorld CollisionWorld;

            [ReadOnly] public ComponentDataFromEntity<Bone.MainEntityLinkData> MainEntities;


            public unsafe void Execute(
                Entity entity, int index,
                [WriteOnly] ref GroundHitResultData ground,
                [ReadOnly] ref GroundHitSphereData sphere,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot
            )
            {
                //var a = new NativeList<DistanceHit>( Allocator.Temp );
                var rtf = new RigidTransform( rot.Value, pos.Value );

                var hitInput = new PointDistanceInput
                {
                    Position = math.transform( rtf, sphere.Center ),
                    MaxDistance = sphere.Distance,
                    Filter = sphere.Filter,
                };
                //var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );// 自身のコライダを除外できればシンプルになるんだが…
                var collector = new AnyHitExcludeSelfCollector<DistanceHit>( sphere.Distance, entity, this.MainEntities );
                var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref collector );

                //var castInput = new RaycastInput
                //{
                //    Start = math.transform( rtf, sphere.Center ),
                //    End = math.transform( rtf, sphere.Center ) + ( math.up() * -sphere.Distance ),
                //    Filter = sphere.filter,
                //};
                //var collector = new AnyHitExcludeSelfCollector2( 1.0f, entity, this.CollisionWorld.Bodies );
                //var isHit = this.CollisionWorld.CastRay( castInput, ref collector );

                //ground = new GroundHitResultData { IsGround = ( isHit && a.Length > 1 ) };
                //ground.IsGround = a.Length > 1;
                ground.IsGround = collector.NumHits > 0;
                //ground.IsGround = isHit;

                //a.Dispose();
            }
        }


    }

}
