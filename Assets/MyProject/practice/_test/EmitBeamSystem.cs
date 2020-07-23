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
    using Abarabone.Geometry;

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
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);

            var bullets = this.GetComponentDataFromEntity<Bullet.BulletData>(isReadOnly: true);

            var tfcam = Camera.main.transform;
            var campos = tfcam.position.As_float3();
            var camrot = new quaternion( tfcam.rotation.As_float4() );


            this.Entities
                .WithBurst()
                .WithReadOnly(handles)
                .WithReadOnly(bullets)
                .ForEach(
                    (
                        Entity fireEntity, int entityInQueryIndex,
                        ref Wapon.BeamUnitData beamUnit,
                        in Rotation rot,
                        in Translation pos
                    ) =>
                    {
                        var handle = handles[beamUnit.MainEntity];
                        if (handle.ControlAction.IsShooting)
                        {
                            var bulletData = bullets[beamUnit.PsylliumPrefab];

                            var ent = cmd.Instantiate(entityInQueryIndex, beamUnit.PsylliumPrefab);

                            cmd.SetComponent(entityInQueryIndex, ent,
                                new Particle.TranslationPtoPData
                                {
                                    Start = math.mul(rot.Value, beamUnit.MuzzlePositionLocal) + pos.Value,
                                    End = campos + math.forward(camrot) * bulletData.RangeDistance,
                                }
                            );
                        }
                    }
                )
                .ScheduleParallel();

            // Make sure that the ECB system knows about our job
            this.cmdSystem.AddJobHandleForProducer(this.Dependency);

        }

    }

}