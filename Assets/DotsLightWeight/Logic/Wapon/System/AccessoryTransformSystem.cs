using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using UnityEngine.XR;
using Unity.Physics.Systems;

namespace DotsLite.Arms
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.ParticleSystem;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using Unity.Physics;
    using DotsLite.Structure;
    using UnityEngine.Rendering;
    using DotsLite.Dependency;
    using DotsLite.Utilities;
    using DotsLite.Targeting;

    using Random = Unity.Mathematics.Random;
    using System;

    // ボーントランスフォームに組み込めればよいが、とりあえず独立で
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    //[UpdateBefore(typeof(MoveSpringSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateBefore(typeof(EmitBulletSystem))]
    [UpdateBefore(typeof(EmitEffectSystem))]
    public partial class AccessoryTransformSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .WithNativeDisableContainerSafetyRestriction(poss)
                .WithNativeDisableContainerSafetyRestriction(rots)
                .ForEach((
                    ref Translation pos,
                    ref Rotation rot,
                    in Emitter.MuzzleTransformData muzzle) =>
                {

                    var parent = muzzle.ParentEntity;
                    var ppos = poss[parent].Value;
                    var prot = rots[parent].Value;

                    pos.Value = ppos + math.mul(prot, muzzle.MuzzlePositionLocal.xyz);
                    rot.Value = prot;

                })
                .ScheduleParallel();

        }

    }
}