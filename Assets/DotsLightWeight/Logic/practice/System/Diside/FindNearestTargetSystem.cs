using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
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
using RaycastHit = Unity.Physics.RaycastHit;

namespace Abarabone.Character
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;
    using Abarabone.Dependency;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class FindNearestTargeSystem : DependencyAccessableSystemBase
    {

        PhysicsHitDependency.Sender phydep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.phydep = PhysicsHitDependency.Sender.Create(this);
        }

        protected override void OnUpdate()
        {
            using var phyScope = this.phydep.WithDependencyScope();


            var cw = this.phydep.PhysicsWorld.CollisionWorld;
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            //this.Entities
            //    .WithBurst()
            //    .ForEach(
            //        (
            //            Entity entity, int entityInQueryIndex,
            //            in TargetSensor.MainLinkData mainlink,
            //            in TargetSensor.FindCollisionData collision
            //        )
            //    =>
            //        {

            //            var startpos = poss[mainlink.MainEntity].Value;

            //            //cw.OverlapSphere(startpos, collision.Distance, );


            //        }
            //    )
            //    .ScheduleParallel();
        }


    }

}

