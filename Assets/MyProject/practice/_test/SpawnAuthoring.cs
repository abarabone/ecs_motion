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
using System;

public class SpawnAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{

    public ModelGroupAuthoring.ModelAuthoringBase prefab;

    public int3 Length;
    public float3 span;


    void IDeclareReferencedPrefabs.DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
    {

        referencedPrefabs.Add( this.prefab.gameObject );

    }

    void IConvertGameObjectToEntity.Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
    {

        var prefab_ent = conversionSystem.GetPrimaryEntity( this.prefab );

        dstManager.AddComponentData(entity,
            new SpawnData
            {
                pos = this.transform.position,
                ent = prefab_ent,
                length = this.Length,
                span = this.span,
            }
        );
        
    }
    

}

public struct SpawnData : IComponentData
{
    public float3 pos;
    public float3 span;
    public Entity ent;
    public int3 length;
    public int i;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ObjectInitializeSystem))]
public class SpawnSystem : SystemBase
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

        this.Entities
            //.WithoutBurst()
            .WithBurst()
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, ref SpawnData spawn) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, spawn.ent);

                    var i = spawn.i;
                    var l = spawn.length;
                    var s = spawn.span;

                    cmd.AddComponent(entityInQueryIndex, ent,
                        new ObjectInitializeData
                        {
                            pos = spawn.pos + new float3(i % l.x * s.x, i / l.x % l.y * s.y, i / (l.x * l.y) * s.z)
                        }
                    );

                    if (++spawn.i >= l.x * l.y * l.z)
                        cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();

        // Make sure that the ECB system knows about our job
        this.cmdSystem.AddJobHandleForProducer(this.Dependency);
    }

}



public struct ObjectInitializeData : IComponentData
{
    public float3 pos;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class ObjectInitializeSystem : SystemBase
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

        this.Entities
            //.WithoutBurst()
            .WithBurst()
            .ForEach(
                (Entity ent, int entityInQueryIndex, ref Translation pos, in ObjectInitializeData init) =>
                {

                    pos.Value = init.pos;

                    cmd.RemoveComponent<ObjectInitializeData>(entityInQueryIndex, ent);

                }
            )
            .ScheduleParallel();


        //var cmd2 = this.cmdSystem.CreateCommandBuffer().ToConcurrent();

        var translations = this.GetComponentDataFromEntity<Translation>();

        this.Entities
            //.WithoutBurst()
            .WithBurst()
            .WithNativeDisableParallelForRestriction(translations)
            .ForEach(
                (Entity ent, int entityInQueryIndex, in ObjectInitializeData init, in ObjectBinder.MainEntityLinkData link) =>
                {

                    var pos = new Translation { Value = init.pos };

                    translations[link.MainEntity] = pos;

                    cmd.RemoveComponent<ObjectInitializeData>(entityInQueryIndex, ent);

                }
            )
            .ScheduleParallel();

        // Make sure that the ECB system knows about our job
        this.cmdSystem.AddJobHandleForProducer(this.Dependency);
    }

}