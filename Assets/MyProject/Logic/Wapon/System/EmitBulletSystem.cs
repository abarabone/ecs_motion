using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    using Random = Unity.Mathematics.Random;


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
            var currentTime = this.Time.ElapsedTime;


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
                        ref Wapon.EmittingStateData state,
                        in Wapon.BulletEmittingData emitter
                    ) =>
                    {
                        var rnd = Random.CreateFromIndex((uint)math.asuint(deltaTime));


                        if (currentTime > state.NextEmitableTime) return;

                        state.NextEmitableTime = (float)currentTime + emitter.EmittingInterval;


                        var handle = handles[emitter.MainEntity];
                        if (!handle.ControlAction.IsShooting) return;


                        var bulletData = bullets[emitter.BulletPrefab];
                        var rot = rots[emitter.MuzzleBodyEntity];
                        var pos = poss[emitter.MuzzleBodyEntity];

                        for (var i=0; i<emitter.NumEmitMultiple; i++)
                        {
                            var bulletPos = calcBulletPosition_(camrot, campos, in emitter, in bulletData);
                            var bulletDir = calcBulletDirection_(camrot, bulletPos, ref rnd, emitter.AccuracyRad);
                            emit_(cmd, entityInQueryIndex, emitter.BulletPrefab, bulletPos, bulletDir, emitter.RangeDistanceFactor, bulletData.RangeDistanceFactor);
                        }
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);

        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcBulletDirection_
            (
                quaternion dirrot, float3 start, ref Random rnd, float accuracyRad
            )
        {

            var yrad = rnd.NextFloat(accuracyRad);
            var zrad = rnd.NextFloat(2.0f * math.PI);
            var bulletDir = math.mul(dirrot, math.forward(quaternion.EulerYZX(0.0f, yrad, zrad)));

            return bulletDir;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float3 calcBulletPosition_
            (
                quaternion camrot, float3 campos, 
                in Wapon.BulletEmittingData emitter, in Bullet.Data bulletData
            )
        {

            //var muzzleDir = math.forward(rot.Value);
            //var start = pos.Value + muzzleDir * emitter.MuzzlePositionLocal;
            var muzzleDir = math.forward(camrot);
            var pos = campos + muzzleDir * 0.5f;

            return pos;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void emit_
            (
                EntityCommandBuffer.ParallelWriter cmd, int entityInQueryIndex, Entity bulletPrefab,
                float3 bulletPosition, float3 bulletDirection, float emitterRangeFactor, float bulletRangeFactor
            )
        {

            var newBullet = cmd.Instantiate(entityInQueryIndex, bulletPrefab);

            cmd.SetComponent(entityInQueryIndex, newBullet,
                new Particle.TranslationPtoPData
                {
                    Start = bulletPosition,
                    End = bulletPosition
                }
            );
            cmd.SetComponent(entityInQueryIndex, newBullet,
                new Bullet.DirectionData
                {
                    Direction = bulletDirection,
                }
            );
            cmd.SetComponent(entityInQueryIndex, newBullet,
                new Bullet.DistanceData
                {
                    RestRangeDistance = emitterRangeFactor * bulletRangeFactor,
                }
            );

        }

    }

}