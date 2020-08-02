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
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.XR;
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
    using Unity.Physics;
    using Abarabone.Structure;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.StructureHitMessage>;
    using Abarabone.SystemGroup.Presentation.DrawModel.MotionBoneTransform;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    public class EmitBeamSystem : SystemBase
    {

        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい

        EntityCommandBufferSystem cmdSystem;

        StructureHitMessageHolderAllocationSystem structureHitHolderSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

            this.buildPhysicsWorldSystem = this.World.GetExistingSystem<BuildPhysicsWorld>();

            this.structureHitHolderSystem = this.World.GetExistingSystem<StructureHitMessageHolderAllocationSystem>();
        }


        struct PtoPUnit
        {
            public float3 start;
            public float3 end;
        }

        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();
            var cw = this.buildPhysicsWorldSystem.PhysicsWorld.CollisionWorld;
            var structureHitHolder = this.structureHitHolderSystem.MsgHolder.AsParallelWriter();


            var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);
            var mainLinks = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);

            var bullets = this.GetComponentDataFromEntity<Bullet.BulletData>(isReadOnly: true);
            var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);


            // カメラは暫定
            var tfcam = Camera.main.transform;
            var campos = tfcam.position.As_float3();
            var camrot = new quaternion( tfcam.rotation.As_float4() );


            this.Entities
                .WithBurst()
                .WithReadOnly(handles)
                .WithReadOnly(bullets)
                .WithReadOnly(mainLinks)
                .WithReadOnly(parts)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref Wapon.BeamEmitterData emitter,
                        in Rotation rot,
                        in Translation pos
                    ) =>
                    {
                        var handle = handles[emitter.MainEntity];
                        if (handle.ControlAction.IsShooting)
                        {
                            var i = entityInQueryIndex;
                            var prefab = emitter.BeamPrefab;
                            var bulletData = bullets[emitter.BeamPrefab];

                            var hit = hitTest_(emitter.MainEntity, camrot, campos, bulletData, ref cw, mainLinks);

                            postMessageToHitTarget_(structureHitHolder, hit, parts);

                            //var (start, end) = calcBeamPosision_(beamUnit, rot, pos, hit, camrot, campos, bulletData);
                            var ptop = calcBeamPosision_(emitter, rot, pos, hit, camrot, campos, bulletData);

                            instantiateBullet_(ref cmd, i, prefab, ptop.start, ptop.end);
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

                if(parts_.HasComponent(hit.hitEntity))
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
            PtoPUnit calcBeamPosision_
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

            void instantiateBullet_
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

}