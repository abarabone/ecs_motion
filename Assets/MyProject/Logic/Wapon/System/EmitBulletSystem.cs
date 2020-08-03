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
    using UnityEngine.Rendering;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.HitSystemGroup))]
    public class EmitBulletSystem : SystemBase
    {

        EntityCommandBufferSystem cmdSystem;

        StructureHitMessageHolderAllocationSystem structureHitHolderSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

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
            var structureHitHolder = this.structureHitHolderSystem.MsgHolder.AsParallelWriter();


            var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);
            //var mainLinks = this.GetComponentDataFromEntity<Bone.MainEntityLinkData>(isReadOnly: true);

            var bullets = this.GetComponentDataFromEntity<Bullet.Data>(isReadOnly: true);
            //var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);


            // カメラは暫定
            var tfcam = Camera.main.transform;
            var campos = tfcam.position.As_float3();
            var camrot = new quaternion( tfcam.rotation.As_float4() );


            this.Entities
                .WithBurst()
                .WithNone<Bullet.Data>()
                .WithReadOnly(handles)
                //.WithReadOnly(mainLinks)
                .WithReadOnly(bullets)
                //.WithReadOnly(parts)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref Wapon.BulletEmittingData emitter,
                        in Rotation rot,
                        in Translation pos
                    ) =>
                    {

                        var handle = handles[emitter.MainEntity];
                        if (!handle.ControlAction.IsShooting) return;


                        var bulletData = bullets[emitter.BulletPrefab];


                        var newBullet = cmd.Instantiate(entityInQueryIndex, emitter.BulletPrefab);

                        //var dir = math.forward(rot.Value);
                        //var start = pos.Value + dir * emitter.MuzzlePositionLocal;
                        //var ptop = new Particle.TranslationPtoPData { Start = start, End = start };
                        var dir = math.forward(camrot);
                        var start = campos + dir * new float3(0.0f, 0.0f, 1.0f);
                        var ptop = new Particle.TranslationPtoPData { Start = start, End = start };

                        cmd.SetComponent(entityInQueryIndex, newBullet, ptop);
                        cmd.SetComponent(entityInQueryIndex, newBullet,
                            new Bullet.DirectionData
                            {
                                Direction = dir,
                            }
                        );

                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);

        }

    }

}