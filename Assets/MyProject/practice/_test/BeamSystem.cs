using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

using Abarabone.Model;
using Abarabone.Model.Authoring;
using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;


//[UpdateInGroup(typeof(InitializationSystemGroup))]
//[UpdateAfter(typeof(ObjectInitializeSystem))]
public class BeamSystem : SystemBase
{

    EntityCommandBufferSystem cmdSystem;


    protected override void OnCreate()
    {
        this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var cmd = this.cmdSystem.CreateCommandBuffer().ToConcurrent();

        this.Entities
            //.WithoutBurst()
            .WithBurst()
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, ref SingleSpawnData spawn) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, spawn.ent);

                    cmd.AddComponent(entityInQueryIndex, ent,
                        new ObjectInitializeData { pos = new float3(spawn.i % 20, spawn.i / 20, 0.0f) }
                        //new ObjectInitializeData { pos = new float3(0,2,0) }
                    );

                    if (--spawn.i == 0)
                        cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();

        // Make sure that the ECB system knows about our job
        this.cmdSystem.AddJobHandleForProducer(this.Dependency);
    }

}
