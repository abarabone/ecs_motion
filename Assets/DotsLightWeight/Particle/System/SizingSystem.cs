using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;


namespace DotsLite.Particle
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Draw;
    using DotsLite.Particle;
    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Collision;
    using DotsLite.SystemGroup;
    using DotsLite.Dependency;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    //[DisableAutoCreation]
    [UpdateAfter(typeof(ParticleLifeTimeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class SizingSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var currentTime = (float)this.Time.ElapsedTime;


            this.Entities
                .WithBurst()
                .ForEach(
                    (
                        ref Particle.AdditionalData data,
                        in BillBoad.SizeAnimationData anim,
                        in Particle.LifeTimeData timer
                    ) =>
                    {

                        var elapsed = currentTime - timer.StartTime;
                        var normalizeTime = math.saturate(elapsed * anim.MaxTimeSpanR);

                        data.Size = math.lerp(anim.StartSize, anim.EndSize, normalizeTime);

                    }
                )
                .ScheduleParallel();
        }

    }


}

