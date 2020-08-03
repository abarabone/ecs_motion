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
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var bullets = this.GetComponentDataFromEntity<Bullet.Data>(isReadOnly: true);
            //var parts = this.GetComponentDataFromEntity<StructurePart.PartData>(isReadOnly: true);


            // カメラは暫定
            var tfcam = Camera.main.transform;
            var campos = tfcam.position.As_float3();
            var camrot = new quaternion( tfcam.rotation.As_float4() );


            var deltaTime = this.Time.DeltaTime;


            this.Entities
                .WithBurst()
                .WithNone<Bullet.Data>()
                .WithReadOnly(handles)
                //.WithReadOnly(mainLinks)
                .WithReadOnly(rots)
                .WithReadOnly(poss)
                .WithReadOnly(bullets)
                //.WithReadOnly(parts)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref Wapon.BulletEmittingData emitter,
                        ref Wapon.EmittingStateData state
                    ) =>
                    {

                        state.RestEmittingInterval -= deltaTime;
                        if (state.RestEmittingInterval > 0.0f) return;

                        state.RestEmittingInterval = emitter.EmittingInterval;


                        var handle = handles[emitter.MainEntity];
                        if (!handle.ControlAction.IsShooting) return;


                        var bulletData = bullets[emitter.BulletPrefab];
                        var rot = rots[emitter.MuzzleBodyEntity];
                        var pos = poss[emitter.MuzzleBodyEntity];


                        var newBullet = cmd.Instantiate(entityInQueryIndex, emitter.BulletPrefab);

                        //var dir = math.forward(rot.Value);
                        //var start = pos.Value + dir * emitter.MuzzlePositionLocal;
                        //var ptop = new Particle.TranslationPtoPData { Start = start, End = start };
                        var dir = math.forward(camrot);
                        var start = campos + dir * 1.0f;
                        var ptop = new Particle.TranslationPtoPData { Start = start, End = start };

                        cmd.SetComponent(entityInQueryIndex, newBullet, ptop);
                        cmd.SetComponent(entityInQueryIndex, newBullet,
                            new Bullet.DirectionData
                            {
                                Direction = dir,
                            }
                        );
                        cmd.SetComponent(entityInQueryIndex, newBullet,
                            new Bullet.DistanceData
                            {
                                RestRangeDistance = emitter.RangeDistanceFactor * bulletData.RangeDistanceFactor,
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