//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Transforms;
//using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
//using Unity.Entities.UniversalDelegates;
//using System.Runtime.InteropServices.WindowsRuntime;
//using System.Runtime.CompilerServices;
//using UnityEngine.XR;
//using Unity.Physics;
//using Unity.Physics.Systems;

//namespace DotsLite.Arms
//{

//    using DotsLite.Model;
//    using DotsLite.Model.Authoring;
//    using DotsLite.Arms;
//    using DotsLite.Character;
//    using DotsLite.ParticleSystem;
//    using DotsLite.SystemGroup;
//    using DotsLite.Geometry;
//    using DotsLite.Structure;
//    using DotsLite.Dependency;
//    using DotsLite.Utilities;
//    using DotsLite.Collision;
//    using DotsLite.Targeting;

//    static class BringYourOwnDelegate
//    {
//        // Declare the delegate that takes 12 parameters. T0 is used for the Entity argument
//        [Unity.Entities.CodeGeneratedJobForEach.EntitiesForEachCompatible]
//        public delegate void CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8>
//            (
//                T0 t0, T1 t1,
//                ref T2 t2,
//                in T3 t3, in T4 t4, in T5 t5, in T6 t6, in T7 t7, in T8 t8
//            );

//        // Declare the function overload
//        public static TDescription ForEach<TDescription, T0, T1, T2, T3, T4, T5, T6, T7, T8>
//            (this TDescription description, CustomForEachDelegate<T0, T1, T2, T3, T4, T5, T6, T7, T8> codeToRun)
//            where TDescription : struct, Unity.Entities.CodeGeneratedJobForEach.ISupportForEachWithUniversalDelegate
//        =>
//            LambdaForEachDescriptionConstructionMethods.ThrowCodeGenException<TDescription>();
//    }

//    //[DisableAutoCreation]
//    //[UpdateInGroup(typeof(SystemGroup.Simulation.Hit.HitSystemGroup))]
//    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
//    [UpdateBefore(typeof(StructureHitMessageApplySystem))]
//    public class EmitAndHitBeamSystem : DependencyAccessableSystemBase
//    {

//        CommandBufferDependency.Sender cmddep;

//        PhysicsHitDependency.Sender phydep;

//        HitMessage<Structure.HitMessage>.Sender stSender;
//        HitMessage<Character.HitMessage>.Sender chSender;


//        //BarrierDependency.Reciever bardep;


//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);

//            this.phydep = PhysicsHitDependency.Sender.Create(this);

//            this.stSender = HitMessage<Structure.HitMessage>.Sender.Create<StructureHitMessageApplySystem>(this);
//            this.chSender = HitMessage<Character.HitMessage>.Sender.Create<CharacterHitMessageApplySystem>(this);

//            //this.bardep = BarrierDependency.Reciever.Create();
//        }


//        struct PtoPUnit
//        {
//            public float3 start;
//            public float3 end;
//        }

//        protected override void OnUpdate()
//        {
//            using var cmdScope = this.cmddep.WithDependencyScope();
//            using var phyScope = this.phydep.WithDependencyScope();
//            using var pthitScope = this.stSender.WithDependencyScope();
//            using var chhitScope = this.chSender.WithDependencyScope();


//            var cmd = cmdScope.CommandBuffer.AsParallelWriter();
//            var cw = phyScope.PhysicsWorld.CollisionWorld;
//            var pthit = pthitScope.MessagerAsParallelWriter;
//            var chhit = chhitScope.MessagerAsParallelWriter;


//            var targets = this.GetComponentDataFromEntity<Hit.TargetData>(isReadOnly: true);
//            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
//            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

//            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);

//            var corpss = this.GetComponentDataFromEntity<CorpsGroup.Data>(isReadOnly: true);


//            // カメラは暫定
//            var tfcam = Camera.main.transform;
//            var campos = tfcam.position.As_float3();
//            var camrot = new quaternion( tfcam.rotation.As_float4() );

//            //this.Dependency =
//            this.Entities
//                .WithBurst()
//                .WithReadOnly(targets)
//                .WithReadOnly(rots)
//                .WithReadOnly(poss)
//                .WithReadOnly(parts)
//                .WithReadOnly(cw)
//                .WithReadOnly(corpss)
//                //.WithReadOnly(cmd)
//                .WithNativeDisableParallelForRestriction(pthit)
//                .WithNativeDisableContainerSafetyRestriction(pthit)
//                .ForEach(
//                    (
//                        Entity fireEntity, int entityInQueryIndex,
//                        ref Emitter.TriggerData trigger,
//                        in Emitter.BulletEmittingData emitter,
//                        in Emitter.BulletMuzzleLinkData mlink,
//                        in Emitter.BulletMuzzlePositionData mzpos,
//                        in Emitter.OwnerLinkData slink,
//                        in Bullet.MoveSpecData bulletData,
//                        in CorpsGroup.TargetWithArmsData corps
//                    ) =>
//                    {
//                        if (!trigger.IsTriggered) return;

//                        trigger.IsTriggered = false;// 逐一オフにする


//                        var i = entityInQueryIndex;
//                        var prefab = emitter.Prefab;
//                        var rot = rots[mlink.MuzzleEntity];
//                        var pos = poss[mlink.MuzzleEntity];
//                        var range = emitter.RangeDistanceFactor * bulletData.RangeDistanceFactor;

//                        var hit_ = hitTest_(slink.StateEntity, camrot, campos, range, ref cw, targets);



//                        //var (start, end) = calcBeamPosision_(emitter.MuzzlePositionLocal, range, rot, pos, hit, camrot, campos);
//                        var ptop = calcBeamPosision_(emitter.MuzzlePositionLocal, range, rot, pos, hit_, camrot, campos);

//                        instantiateBullet_(ref cmd, i, prefab, slink.StateEntity, ptop.start, ptop.end);



//                        if (!hit_.isHit) return;

//                        var hit = hit_.core;

//                        switch (hit.hitType)
//                        {
//                            case HitType.part:

//                                hit.PostStructureHitMessage(sthit, parts);
//                                break;


//                            case HitType.charactor:

//                                var otherCorpts = corpss[hit.hitEntity];
//                                if ((otherCorpts.BelongTo & corps.TargetCorps) == 0) return;

//                                hit.PostCharacterHitMessage(chhit, 1.0f, 0.0f);
//                                break;


//                            default:
//                                break;
//                        }

//                    }
//                )
//                .ScheduleParallel();// this.buildPhysicsWorldSystem.GetOutputDependency());
//        }



//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static BulletHitUtility.BulletHit hitTest_
//            (
//                Entity stateEntity, quaternion sightRot, float3 sightPos, float range,
//                ref CollisionWorld cw_,
//                ComponentDataFromEntity<Hit.TargetData> targets
//            )
//        {
//            var sightDir = math.forward(sightRot);
//            var hitStart = sightPos + sightDir * 1.0f;
//            var hitEnd = sightPos + sightDir * range;

//            return cw_.BulletHitRay(stateEntity, hitStart, hitEnd, range, targets);
//        }



//        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
//        //static void postMessageToHitTarget_
//        //    (
//        //        StructureHitHolder.ParallelWriter structureHitHolder_,
//        //        BulletHitUtility.BulletHit hit,
//        //        ComponentDataFromEntity<StructurePart.PartData> parts_
//        //    )
//        //{
//        //    if (!hit.isHit) return;

//        //    if (parts_.HasComponent(hit.hitEntity))
//        //    {
//        //        structureHitHolder_.Add(hit.mainEntity,
//        //            new StructureHitMessage
//        //            {
//        //                Position = hit.posision,
//        //                Normale = hit.normal,
//        //                PartEntity = hit.hitEntity,
//        //                PartId = parts_[hit.hitEntity].PartId,
//        //            }
//        //        );
//        //    }
//        //}



//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        //(float3 start, float3 end) calcBeamPosision_
//        static PtoPUnit calcBeamPosision_
//            (
//                float3 muzzlePositionLocal, float range,
//                Rotation mainrot, Translation mainpos, BulletHitUtility.BulletHit hit,
//                quaternion sightRot, float3 sightPos
//            )
//        {

//            var beamStart = math.mul(mainrot.Value, muzzlePositionLocal) + mainpos.Value;

//            //if (hit.isHit) return (beamStart, hit.posision);
//            if (hit.isHit) return new PtoPUnit { start = beamStart, end = hit.core.posision };


//            var beamEnd = sightPos + math.forward(sightRot) * range;

//            //return (beamStart, beamEnd);
//            return new PtoPUnit { start = beamStart, end = beamEnd };
//        }



//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static void instantiateBullet_
//            (
//                ref EntityCommandBuffer.ParallelWriter cmd, int i, Entity bulletPrefab, Entity stateEntity,
//                float3 start, float3 end
//            )
//        {
//            var newBeamEntity = cmd.Instantiate(i, bulletPrefab);

//            //cmd.SetComponent(i, newBeamEntity,
//            //    new Particle.TranslationPtoPData
//            //    {
//            //        Start = start,
//            //        End = end,
//            //    }
//            //);
//            cmd.SetComponent(i, newBeamEntity,
//                new Psyllium.TranslationTailData
//                {
//                    PositionAndSize = end.As_float4(),
//                }
//            );
//            cmd.SetComponent(i, newBeamEntity,
//                new Translation
//                {
//                    Value = start,
//                }
//            );
//            cmd.SetComponent(i, newBeamEntity,
//                new Bullet.LinkData
//                {
//                    OwnerStateEntity = stateEntity,
//                }
//            );

//        }
//    }

//}