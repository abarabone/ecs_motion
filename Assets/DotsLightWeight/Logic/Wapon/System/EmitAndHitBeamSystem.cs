﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.CompilerServices;
using UnityEngine.XR;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Particle;
    using Abarabone.SystemGroup;
    using Abarabone.Geometry;
    using Abarabone.Structure;
    using Abarabone.Dependency;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.StructureHitMessage>;
    using Abarabone.SystemGroup.Presentation.DrawModel.MotionBoneTransform;


    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    [UpdateBefore(typeof(StructureHitMessageApplySystem))]
    public class EmitAndHitBeamSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;

        PhysicsHitDependency.Sender phydep;

        HitMessage<StructureHitMessage>.Sender sender;

        //BarrierDependency.Reciever bardep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

            this.phydep = PhysicsHitDependency.Sender.Create(this);

            this.sender = HitMessage<StructureHitMessage>.Sender.Create<StructureHitMessageApplySystem>(this);

            //this.bardep = BarrierDependency.Reciever.Create();
        }


        struct PtoPUnit
        {
            public float3 start;
            public float3 end;
        }

        protected override void OnUpdate()
        {
            using var phyScope = this.phydep.WithDependencyScope();
            using var hitScope = this.sender.WithDependencyScope();
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();
            var cw = this.phydep.PhysicsWorld.CollisionWorld;
            var sthit = this.sender.AsParallelWriter();


            var mainLinks = this.GetComponentDataFromEntity<Bone.PostureLinkData>(isReadOnly: true);
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);


            // カメラは暫定
            var tfcam = Camera.main.transform;
            var campos = tfcam.position.As_float3();
            var camrot = new quaternion( tfcam.rotation.As_float4() );

            //this.Dependency =
            this.Entities
                .WithBurst()
                .WithReadOnly(mainLinks)
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                .WithReadOnly(parts)
                .WithReadOnly(cw)
                //.WithReadOnly(cmd)
                .WithNativeDisableParallelForRestriction(sthit)
                .WithNativeDisableContainerSafetyRestriction(sthit)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        in FunctionUnit.BulletEmittingData emitter,
                        in FunctionUnit.TriggerData trigger,
                        in FunctionUnit.OwnerLinkData link,
                        in Bullet.SpecData bulletData
                    ) =>
                    {
                        if (!trigger.IsTriggered) return;

                        var i = entityInQueryIndex;
                        var prefab = emitter.BulletPrefab;
                        var rot = rots[link.MuzzleEntity];
                        var pos = poss[link.MuzzleEntity];
                        var range = emitter.RangeDistanceFactor * bulletData.RangeDistanceFactor;

                        var hit = hitTest_(link.OwnerMainEntity, camrot, campos, range, ref cw, mainLinks);



                        //var (start, end) = calcBeamPosision_(emitter.MuzzlePositionLocal, range, rot, pos, hit, camrot, campos);
                        var ptop = calcBeamPosision_(emitter.MuzzlePositionLocal, range, rot, pos, hit, camrot, campos);

                        instantiateBullet_(ref cmd, i, prefab, ptop.start, ptop.end);



                        if (!hit.isHit) return;

                        hit.postMessageToHitTarget(sthit, parts);

                    }
                )
                .ScheduleParallel();// this.buildPhysicsWorldSystem.GetOutputDependency());
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static BulletHitUtility.BulletHit hitTest_
            (
                Entity mainEntity, quaternion sightRot, float3 sightPos, float range,
                ref CollisionWorld cw_,
                ComponentDataFromEntity<Bone.PostureLinkData> mainLinks_
            )
        {
            var sightDir = math.forward(sightRot);
            var hitStart = sightPos + sightDir * 1.0f;
            var hitEnd = sightPos + sightDir * range;

            return cw_.BulletHitRay(mainEntity, hitStart, hitEnd, range, mainLinks_);
        }



        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static void postMessageToHitTarget_
        //    (
        //        StructureHitHolder.ParallelWriter structureHitHolder_,
        //        BulletHitUtility.BulletHit hit,
        //        ComponentDataFromEntity<StructurePart.PartData> parts_
        //    )
        //{
        //    if (!hit.isHit) return;

        //    if (parts_.HasComponent(hit.hitEntity))
        //    {
        //        structureHitHolder_.Add(hit.mainEntity,
        //            new StructureHitMessage
        //            {
        //                Position = hit.posision,
        //                Normale = hit.normal,
        //                PartEntity = hit.hitEntity,
        //                PartId = parts_[hit.hitEntity].PartId,
        //            }
        //        );
        //    }
        //}



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //(float3 start, float3 end) calcBeamPosision_
        static PtoPUnit calcBeamPosision_
            (
                float3 muzzlePositionLocal, float range,
                Rotation mainrot, Translation mainpos, BulletHitUtility.BulletHit hit,
                quaternion sightRot, float3 sightPos
            )
        {

            var beamStart = math.mul(mainrot.Value, muzzlePositionLocal) + mainpos.Value;

            //if (hit.isHit) return (beamStart, hit.posision);
            if (hit.isHit) return new PtoPUnit { start = beamStart, end = hit.posision };


            var beamEnd = sightPos + math.forward(sightRot) * range;

            //return (beamStart, beamEnd);
            return new PtoPUnit { start = beamStart, end = beamEnd };
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void instantiateBullet_
            (
                ref EntityCommandBuffer.ParallelWriter cmd_, int i, Entity bulletPrefab,
                float3 start, float3 end
            )
        {
            var newBeamEntity = cmd_.Instantiate(i, bulletPrefab);

            cmd_.SetComponent(i, newBeamEntity,
                new Particle.TranslationPtoPData
                {
                    Start = start,
                    End = end,
                }
            );
        }
    }

}