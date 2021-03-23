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

namespace Abarabone.Character
{
    using Abarabone.Common;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Geometry;
    using Abarabone.Physics;
    using Abarabone.Model;


    using Abarabone.CharacterMotion;

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
    public class FreeFallWithHitSystem : PhysicsHitSystemBase
    {

        protected override void OnUpdateWith(BuildPhysicsWorld physicsBuilder)
        {
            var mainEntities = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);
            var collisionWorld = physicsBuilder.PhysicsWorld;

            this.Entities
                .WithBurst()
                .WithReadOnly(mainEntities)
                .WithReadOnly(collisionWorld)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref WallHitResultData result,
                        ref Translation pos,
                        ref Rotation rot,
                        in GroundHitSphereData sphere
                    )
                =>
                    {
                        //var rtf = new RigidTransform( rot.Value, pos.Value );

                        var hitInput = new PointDistanceInput
                        {
                            Position = pos.Value,//math.transform( rtf, sphere.Center ),
                            MaxDistance = sphere.Distance,
                            Filter = sphere.Filter,
                        };
                        //var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );// 自身のコライダを除外できればシンプルになるんだが…

                        var collector = new ClosestHitExcludeSelfCollector<DistanceHit>(sphere.Distance, entity, mainEntities);
                        //var collector = new ClosestHitCollector<DistanceHit>( sphere.Distance );
                        var isHit = collisionWorld.CalculateDistance(hitInput, ref collector);

                        if (collector.NumHits == 0) return;


                        result.IsHit = true;

                        var n = collector.ClosestHit.SurfaceNormal;
                        var p = collector.ClosestHit.Position;
                        pos.Value = p + n * sphere.Distance;

                        var right = math.mul(rot.Value, new float3(1.0f, 0.0f, 0.0f));
                        var forward = math.cross(n, right);
                        var safe_forward = math.select(math.forward(rot.Value), forward, math.dot(right, n) > 0.001f);
                        rot.Value = quaternion.LookRotation(safe_forward, n);
                    }
                )
                .ScheduleParallel();
        }

    }
}
