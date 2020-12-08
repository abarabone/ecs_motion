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


namespace Abarabone.Model
{

    
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;

    using LeveledLinkData = Bone.Lv03LinkData;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    [UpdateAfter(typeof(BoneTransformLeveld02System))]
    public class BoneTransformLeveld03System : SystemBase
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
                .WithNone<TransformOption.ExcludeTransformTag>()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .WithReadOnly(masses)
                .WithNativeDisableParallelForRestriction(velocities)
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
                            velocities[entity] = BoneUtility.BoneTransform
                                (in parentpos, in parentrot, in local, in pos, in rot, in mass, deltaTime);
                        }
                        else
                        {
                            BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                        }
                    }
                )
                .ScheduleParallel();
        }

    }


}