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

    using LeveledLinkData = Bone.Lv01LinkData;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    [UpdateAfter(typeof(StreamToBoneSystem))]
    public class BoneTransformLeveld01System : SystemBase
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
                .WithNativeDisableContainerSafetyRestriction(poss)
                .WithNativeDisableContainerSafetyRestriction(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,// ref PhysicsVelocity v,
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

                        //BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                        //var ppos = parentpos.Value;
                        //var prot = parentrot.Value;

                        //var lpos = local.Position;
                        //var lrot = local.Rotation;

                        //var newpos = math.mul(prot, lpos) + ppos;
                        //var newrot = math.mul(prot, lrot);
                        //pos = new Translation { Value = newpos };
                        //rot = new Rotation { Value = newrot };
                        //v.Linear = pos.Value
                    }
                )
                .ScheduleParallel();
        }

    }


}