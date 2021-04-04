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
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.XR;
using Unity.Physics.Systems;

namespace Abarabone.Arms
{
    using Abarabone.Common;
    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Particle;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Unity.Physics;
    using Abarabone.Structure;
    using Abarabone.Character.Action;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.StructureHitMessage>;
    using Abarabone.SystemGroup.Presentation.DrawModel.MotionBoneTransform;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    //[UpdateAfter(typeof(BulletMoveSystem))]
    //[UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class BulletHitSystem : PhysicsHitSystemBase
    {


        //StructureHitMessageHolderAllocationSystem structureHitHolderSystem;
        BulletHitApplyToCharacterSystem hitChSystem;//


        protected override void OnCreate()
        {
            base.OnCreate();

            //this.structureHitHolderSystem = this.World.GetExistingSystem<StructureHitMessageHolderAllocationSystem>();
            this.hitChSystem = this.World.GetExistingSystem<BulletHitApplyToCharacterSystem>();//
        }


        protected override void OnUpdateWith(BuildPhysicsWorld physicsBuilder)
        {
            //var structureHitHolder = this.structureHitHolderSystem.MsgHolder.AsParallelWriter();
            var chhit_ = this.hitChSystem.GetParallelWriter();//
            var cw = physicsBuilder.PhysicsWorld.CollisionWorld;

            var mainLinks = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithReadOnly(mainLinks)
                .WithReadOnly(parts)
                .WithReadOnly(cw)
                //.WithNativeDisableContainerSafetyRestriction(structureHitHolder)
                //.WithNativeDisableParallelForRestriction(structureHitHolder)
                .ForEach(
                    (
                        Entity fireEntity,// int entityInQueryIndex,
                        in Particle.TranslationPtoPData ptop,
                        in Bullet.SpecData bullet,
                        in Bullet.DistanceData dist
                    ) =>
                    {
                        var hit = cw.BulletHitRay
                            (bullet.MainEntity, ptop.Start, ptop.End, dist.RestRangeDistance, mainLinks);

                        //hit.postMessageToHitTarget(structureHitHolder, parts);
                        hit.postMessageToHitTarget(chhit_, parts);
                    }
                )
                .ScheduleParallel();

            this.hitChSystem.AddDependencyBeforeHitApply(this.Dependency);
        }

    }

}