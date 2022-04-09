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
    using DotsLite.Model.Authoring;
    using System.Security;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.Render.Draw.Transform.MotionBone))]
    [UpdateAfter(typeof( StreamToBoneSystem ) )]
    public partial class BoneTransformSystem : SystemBase//JobComponentSystem
    {


        protected override void OnUpdate()
        {

            var boneLinkers = this.GetComponentDataFromEntity<Bone.RelationLinkData>( isReadOnly: true );
            var locals = this.GetComponentDataFromEntity<Bone.LocalValueData>( isReadOnly: true );
            var poss = this.GetComponentDataFromEntity<Translation>();
            var rots = this.GetComponentDataFromEntity<Rotation>();
            var velocities = this.GetComponentDataFromEntity<PhysicsVelocity>();
            var masses = this.GetComponentDataFromEntity<PhysicsMass>( isReadOnly: true );

            var deltaTime = this.Time.DeltaTime;

            this.Entities
                .WithBurst()
                .WithAll<Bone.TransformTargetTag>()
                .WithReadOnly(boneLinkers)
                .WithReadOnly(locals)
                .WithReadOnly(masses)
                .WithNativeDisableParallelForRestriction(poss)
                .WithNativeDisableParallelForRestriction(rots)
                .WithNativeDisableContainerSafetyRestriction(poss)
                .WithNativeDisableContainerSafetyRestriction(rots)
                //.WithNativeDisableParallelForRestriction( velocities )
                .ForEach(
                        (
                            //Entity drawent,
                            ref Translation pos,
                            ref Rotation rot,
                            in DrawInstance.BoneLinkData bonelink,
                            in DrawInstance.TargetWorkData target,
                            in DrawInstance.PostureLinkData posturelink,
                            in DrawInstance.TransformOffsetData offset
                        ) =>
                    {

                        if (target.DrawInstanceId == -1) return;



                        var rootpos = poss[posturelink.PostureEntity];
                        var rootrot = rots[posturelink.PostureEntity];
                        pos.Value = math.mul(rootrot.Value, offset.Position) + rootpos.Value;
                        rot.Value = math.mul(offset.Rotation, rootrot.Value);
                        //poss[drawent] = new Translation { Value = offset.Position + rootpos.Value };
                        //rots[drawent] = new Rotation { Value = math.mul(offset.Rotation, rootrot.Value) };



                        for (
                            var entity = bonelink.BoneRelationTop;
                            entity != Entity.Null;
                            entity = boneLinkers[ entity ].NextBoneEntity
                        )
                        {

                            var parentEntity = boneLinkers[entity].ParentBoneEntity;

                            var parentpos = poss[parentEntity];
                            var parentrot = rots[parentEntity];
                            var local = locals[entity];

                            BoneUtility.transform_(in parentpos, in parentrot, in local, out var newpos, out var newrot);


                            var isNotPhysicsOrStatic = !masses.HasComponent(entity);
                            if (isNotPhysicsOrStatic)
                            {
                                poss[entity] = new Translation { Value = newpos };
                                rots[entity] = new Rotation { Value = newrot };

                                continue;
                            }

                            // キネマティックは未完成
                            // とりあえず静的と一緒にしてある
                            var isKinematic = masses[entity].InverseMass == 0.0f;
                            if (isKinematic)
                            {
                                poss[entity] = new Translation { Value = newpos };
                                rots[entity] = new Rotation { Value = newrot };

                                //var mass = masses[entity];
                                //var toRt = new RigidTransform(newrot, newpos);
                                //var frompos = poss[entity];
                                //var fromrot = rots[entity];
                                //velocities[entity] =
                                //    PhysicsVelocity.CalculateVelocityToTarget
                                //    (in mass, in frompos, in fromrot, in toRt, deltaTime);

                                continue;
                            }
                        }
                    }
                )
                .ScheduleParallel();

        }
        

    }
    
}
