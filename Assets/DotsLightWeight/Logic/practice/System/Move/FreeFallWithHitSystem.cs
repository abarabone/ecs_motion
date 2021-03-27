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
                        ref PhysicsVelocity v,
                        in GroundHitWallingData walling
                    )
                =>
                    {
                        //var rtf = new RigidTransform( rot.Value, pos.Value );
                        var center = pos.Value + walling.CenterHeight * math.mul(rot.Value, math.up());

                        var hitInput = new PointDistanceInput
                        {
                            Position = center,
                            MaxDistance = walling.HangerRange,//.CenterHeight,//.HangerRange,
                            Filter = walling.Filter,
                        };
                        var collector = new ClosestHitExcludeSelfCollector<DistanceHit>(1.0f, entity, mainEntities);
                        var isHit = collisionWorld.CalculateDistance(hitInput, ref collector);

                        if (collector.NumHits <= 0) return;
                        //if (!isHit) return;


                        result.IsHit = true;

                        var n = collector.ClosestHit.SurfaceNormal;
                        var p = collector.ClosestHit.Position;
                        pos.Value = p;

                        var right = math.mul(rot.Value, new float3(1.0f, 0.0f, 0.0f));
                        var forward = math.cross(n, right);
                        var safe_forward = math.select(math.forward(rot.Value), forward, math.abs(math.dot(right, n)) > math.FLT_MIN_NORMAL);
                        rot.Value = quaternion.LookRotation(safe_forward, n);
                        //rot.Value = quaternion.LookRotationSafe(forward, n);

                        v.Linear = 0;
                        v.Angular = 0;
                    }
                )
                .ScheduleParallel();
        }

    }
}
