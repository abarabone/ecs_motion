using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;


namespace DotsLite.Model
{

    
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;

    using LeveledLinkData = Bone.Lv03LinkData;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transform.MotionBone))]
    [UpdateAfter(typeof(BoneTransformLeveld02System))]
    public partial class BoneTransformLeveld03System : SystemBase
    {
        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var velocities = this.GetComponentDataFromEntity<PhysicsVelocity>();
            var masses = this.GetComponentDataFromEntity<PhysicsMass>(isReadOnly: true);

            var deltaTime = this.Time.DeltaTime;


            this.Entities
                .WithBurst()
                .WithAll<Bone.TransformTargetTag>()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .WithReadOnly(masses)
                .WithNativeDisableParallelForRestriction(velocities)
                .WithNativeDisableContainerSafetyRestriction(poss)
                .WithNativeDisableContainerSafetyRestriction(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in LeveledLinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        var im = masses.HasComponent(entity) ? masses[entity].InverseMass : 0.0f;
                        if (im != 0.0f && velocities.HasComponent(entity))
                        {
                            var mass = masses[entity];
                            velocities[entity] = local.BoneTransform
                                (in parentpos, in parentrot, in pos, in rot, in mass, deltaTime);
                        }
                        else
                        {
                            local.BoneTransform(in parentpos, in parentrot, ref pos, ref rot);
                        }
                    }
                )
                .ScheduleParallel();
        }

    }


}