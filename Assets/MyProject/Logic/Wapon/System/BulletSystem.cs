using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;


namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Draw;
    using Abarabone.Particle;
    using Abarabone.CharacterMotion;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Physics;
    using Abarabone.SystemGroup;
    using Abarabone.Structure;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.StructureHitMessage>;
    using Abarabone.SystemGroup.Presentation.DrawModel.MotionBoneTransform;


    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(ObjectInitializeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    //[UpdateAfter(typeof())]
    public class BulletSystem : SystemBase
    {


        EntityCommandBufferSystem cmdSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();


            //var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);

            var deltaTime = this.Time.DeltaTime;


            this.Entities
                //.WithoutBurst()
                .WithBurst()
                .WithAll<Bullet.BeamTag>()
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref Bullet.BulletData beam,
                        ref Particle.AdditionalData additional
                    ) =>
                    {

                        beam.LifeTime -= deltaTime;

                        var transparency = math.max(beam.LifeTime, 0.0f) * beam.InvTotalTime;
                        additional.Color = additional.Color.ApplyAlpha(transparency);//(additional.Color.to_float4() * dc).ToColor32();

                        if (beam.LifeTime <= 0.0f)
                        {
                            cmd.DestroyEntity(entityInQueryIndex, entity);
                        }
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);

            return;


            BulletHitUtility.BulletHit hitTest_
                (
                    Entity mainEntity, quaternion sightRot, float3 sightPos,
                    Bullet.BulletData bulletData,
                    ref CollisionWorld cw_,
                    ComponentDataFromEntity<Bone.MainEntityLinkData> mainLinks_
                )
            {
                var sightDir = math.forward(sightRot);
                var hitStart = sightPos + sightDir * 1.0f;
                var hitEnd = sightPos + sightDir * bulletData.RangeDistance;
                var distance = bulletData.RangeDistance;

                return cw_.BulletHitRay(mainEntity, hitStart, hitEnd, distance, mainLinks_);
            }

            void postMessageToHitTarget_
                (
                    StructureHitHolder.ParallelWriter structureHitHolder_,
                    BulletHitUtility.BulletHit hit,
                    ComponentDataFromEntity<StructurePart.PartData> parts_
                )
            {
                if (!hit.isHit) return;

                if (parts_.HasComponent(hit.hitEntity))
                {
                    structureHitHolder.Add(hit.mainEntity,
                        new StructureHitMessage
                        {
                            Position = hit.posision,
                            Normale = hit.normal,
                            PartEntity = hit.hitEntity,
                            PartId = parts_[hit.hitEntity].PartId,
                        }
                    );
                }
            }

            //(float3 start, float3 end) calcBeamPosision_
            PtoPUnit calcBulletPosision_
                (
                    Wapon.BeamEmitterData beamUnit,
                    Rotation mainrot, Translation mainpos, BulletHitUtility.BulletHit hit,
                    quaternion sightRot, float3 sightPos, Bullet.BulletData bulletData
                )
            {

                var beamStart = math.mul(mainrot.Value, beamUnit.MuzzlePositionLocal) + mainpos.Value;

                //if (hit.isHit) return (beamStart, hit.posision);
                if (hit.isHit) return new PtoPUnit { start = beamStart, end = hit.posision };


                var beamEnd = sightPos + math.forward(sightRot) * bulletData.RangeDistance;

                //return (beamStart, beamEnd);
                return new PtoPUnit { start = beamStart, end = beamEnd };
            }

        }

    }


}

