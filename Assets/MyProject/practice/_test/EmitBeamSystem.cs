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


namespace Abarabone.Character
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Particle;
    using Abarabone.SystemGroup;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class EmitBeamSystem : SystemBase
    {

        EntityCommandBufferSystem cmdSystem;


        protected override void OnCreate()
        {
            this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            var cmd = this.cmdSystem.CreateCommandBuffer().ToConcurrent();


            var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);

            var bullets = this.GetComponentDataFromEntity<Bullet.BulletData>();
            var ptops = this.GetComponentDataFromEntity<Particle.TranslationPtoPData>();


            //this.Entities
            //    .WithBurst()
            //    .WithReadOnly(handles)
            //    .WithNativeDisableParallelForRestriction(bullets)
            //    .WithNativeDisableParallelForRestriction(ptops)
            //    .ForEach(
            //        (
            //            Entity fireEntity, int entityInQueryIndex,
            //            ref Wapon.BeamUnitData beamUnit,
            //            in Rotation rot,
            //            in Translation pos
            //        ) =>
            //        {
            //            var handle = handles[beamUnit.MainEntity];
            //            if (handle.ControlAction.IsShooting)
            //            {
            //                if (beamUnit.BeamInstanceEntity == Entity.Null)
            //                {
            //                    return;
            //                }

            //                var bulletent = beamUnit.BeamInstanceEntity;

            //                bullets[bulletent] = new Bullet.BulletData
            //                {
            //                    LifeTime = 0.3f,
            //                };
            //                ptops[bulletent] = new Particle.TranslationPtoPData
            //                {
            //                    Start = pos.Value,
            //                    End = math.mul(rot.Value, pos.Value + beamUnit.MuzzlePositionLocal),
            //                };
            //            }
            //        }
            //    )
            //    .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(handles)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref Wapon.BeamUnitData beamUnit
                    ) =>
                    {
                        var handle = handles[beamUnit.MainEntity];
                        if (handle.ControlAction.IsShooting)
                        {
                            if (beamUnit.BeamInstanceEntity == Entity.Null)
                            {
                                //beamUnit.BeamInstanceEntity =
                                //    cmd.Instantiate(entityInQueryIndex, beamUnit.PsylliumPrefab);
                                emit_(entityInQueryIndex, ref beamUnit);
                            }
                        }
                        else
                        {
                            if (beamUnit.BeamInstanceEntity != Entity.Null)
                            {
                                cmd.DestroyEntity(entityInQueryIndex, beamUnit.BeamInstanceEntity);
                                beamUnit.BeamInstanceEntity = Entity.Null;
                            }
                        }
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);

            return;


            void emit_(int entityInQueryIndex_, ref Wapon.BeamUnitData beamUnit_)
            {

                var ent = cmd.Instantiate(entityInQueryIndex_, beamUnit_.PsylliumPrefab);

                cmd.AddComponent(entityInQueryIndex_, ent, new Bullet.BeamTag { });
                cmd.AddComponent(entityInQueryIndex_, ent, new Bullet.BulletData { });

                beamUnit_.BeamInstanceEntity = ent;
            }

        }

    }

}