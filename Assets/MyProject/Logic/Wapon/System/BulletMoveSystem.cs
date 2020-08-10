using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;


namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Draw;
    using Abarabone.Particle;
    using Abarabone.CharacterMotion;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Physics;
    using Abarabone.SystemGroup;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(ObjectInitializeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    //[UpdateAfter(typeof())]
    public class BulletMoveSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var deltaTime = this.Time.DeltaTime;


            this.Entities
                .WithBurst()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Particle.TranslationPtoPData ptop,
                        ref Bullet.DistanceData dist,
                        in Bullet.SpecData bullet,
                        in Bullet.DirectionData dir
                    ) =>
                    {

                        var d = bullet.BulletSpeed * deltaTime;

                        ptop.Start = ptop.End;

                        ptop.End += dir.Direction * d;

                        dist.RestRangeDistance -= d;

                    }
                )
                .ScheduleParallel();

        }

    }


}

