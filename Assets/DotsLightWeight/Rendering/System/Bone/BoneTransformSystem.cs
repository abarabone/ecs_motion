﻿using System.Collections;
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
    using Abarabone.Model.Authoring;
    using System.Security;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup ))]
    [UpdateAfter(typeof( StreamToBoneSystem ) )]
    public class BoneTransformSystem : SystemBase//JobComponentSystem
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
                .WithNone<TransformOption.ExcludeTransformTag>()
                .WithReadOnly( boneLinkers )
                .WithReadOnly( locals )
                .WithReadOnly( masses )
                .WithNativeDisableParallelForRestriction( poss )
                .WithNativeDisableParallelForRestriction( rots )
                //.WithNativeDisableParallelForRestriction( velocities )
                .ForEach(
                        (
                            in DrawInstance.BoneLinkData linker,
                            in DrawInstance.TargetWorkData target
                        ) =>
                    {

                        if (target.DrawInstanceId == -1) return;

                        for(
                            var entity = linker.BoneRelationTop;
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
