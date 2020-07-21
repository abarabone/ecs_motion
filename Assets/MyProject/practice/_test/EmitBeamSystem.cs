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

using Abarabone.Model;
using Abarabone.Model.Authoring;
using Abarabone.Arms;
using Abarabone.Character;

//[UpdateInGroup(typeof(InitializationSystemGroup))]
//[UpdateAfter(typeof(ObjectInitializeSystem))]
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

        var psylliums = this.GetComponentDataFromEntity<Wapon.BeamUnitData>();
        var handles = this.GetComponentDataFromEntity<MoveHandlingData>(isReadOnly: true);

        this.Entities
            //.WithoutBurst()
            .WithBurst()
            .ForEach(
                (Entity fireEntity, int entityInQueryIndex, ref Wapon.BeamUnitData beamUnit) =>
                {
                    var ent = psylliums.Exists(beamUnit.BeamInstanceEntity)
                        ? psylliums.t
                    var ent = cmd.Instantiate(entityInQueryIndex, beamUnit.PsylliumPrefab);


                }
            )
            .ScheduleParallel();

        // Make sure that the ECB system knows about our job
        this.cmdSystem.AddJobHandleForProducer(this.Dependency);
    }

}
