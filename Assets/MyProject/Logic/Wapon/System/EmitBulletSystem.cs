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


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    public class EmitBulletSystem : SystemBase
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
                        ref Wapon.BeamEmitterData beamUnit,
                        in Rotation rot,
                        in Translation pos
                    ) =>
                    {
                        var handle = handles[beamUnit.MainEntity];
                        if (handle.ControlAction.IsShooting)
                        {
                            var i = entityInQueryIndex;
                            var prefab = beamUnit.BeamPrefab;
                            var bulletData = bullets[beamUnit.BeamPrefab];

                            instantiateBullet_(ref cmd, i, prefab, ptop.start, ptop.end);
                        }
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);

            return;


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