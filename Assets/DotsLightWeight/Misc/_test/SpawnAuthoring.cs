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
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System;
using Abarabone.Dependency;

public class SpawnAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{

    public ModelGroupAuthoring.ModelAuthoringBase prefab;

    public int3 Length;
    public float3 span;


    public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
    {

        referencedPrefabs.Add( this.prefab.gameObject );

    }

    public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
    {

        var prefab_ent = conversionSystem.GetPrimaryEntity( this.prefab );

        dstManager.AddComponentData(entity,
            new Spawn.EntryData
            {
                pos = this.transform.position,
                prefab = prefab_ent,
            }
        );
        dstManager.AddComponentData(entity,
            new Spawn.SpanData
            {
                length = this.Length,
                span = this.span,
            }
        );

    }
    

}

static class Spawn
{
    public struct EntryData : IComponentData
    {
        public float3 pos;
        public quaternion rot;
        public Entity prefab;
    }
    public struct SpanData : IComponentData
    {
        public float3 span;
        public int3 length;
        public int i;
    }
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ObjectInitializeSystem))]
public class SpawnFreqencySystem : DependencyAccessableSystemBase
{


    CommandBufferDependencySender cmddep;


    protected override void OnCreate()
    {
        base.OnCreate();

        this.cmddep = CommandBufferDependencySender.Create<BeginInitializationEntityCommandBufferSystem>(this);
    }

    protected override void OnUpdate()
    {
        using var cmdScope = this.cmddep.WithDependencyScope();


        var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

        this.Entities
            .WithBurst()
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, ref Spawn.SpanData span, in Spawn.EntryData entry) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, entry.prefab);

                    var i = span.i;
                    var l = span.length;
                    var s = span.span;

                    cmd.AddComponent(entityInQueryIndex, ent,
                        new ObjectInitializeData
                        {
                            pos = entry.pos + new float3(i % l.x * s.x, i / l.x % l.y * s.y, i / (l.x * l.y) * s.z),
                            rot = math.any(entry.rot.value) ? entry.rot : quaternion.identity,
                        }
                    );

                    if (++span.i >= l.x * l.y * l.z)
                        cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();
    }

}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ObjectInitializeSystem))]
public class SpawnSystem : DependencyAccessableSystemBase
{


    CommandBufferDependencySender cmddep;


    protected override void OnCreate()
    {
        base.OnCreate();

        this.cmddep = CommandBufferDependencySender.Create<BeginInitializationEntityCommandBufferSystem>(this);
    }

    protected override void OnUpdate()
    {
        using var cmdScope = this.cmddep.WithDependencyScope();


        var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

        this.Entities
            .WithBurst()
            .WithNone<Spawn.SpanData>()
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, in Spawn.EntryData entry) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, entry.prefab);

                    cmd.AddComponent(entityInQueryIndex, ent,
                        new ObjectInitializeData
                        {
                            pos = entry.pos,
                            rot = math.any(entry.rot.value) ? entry.rot : quaternion.identity,
                        }
                    );

                    cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();
    }

}


public struct ObjectInitializeData : IComponentData
{
    public float3 pos;
    public quaternion rot;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class ObjectInitializeSystem : DependencyAccessableSystemBase
{


    CommandBufferDependencySender cmddep;


    protected override void OnCreate()
    {
        base.OnCreate();

        this.cmddep = CommandBufferDependencySender.Create<BeginInitializationEntityCommandBufferSystem>(this);
    }

    protected override void OnUpdate()
    {
        using var cmdScope = this.cmddep.WithDependencyScope();


        var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

        this.Entities
            //.WithoutBurst()
            .WithBurst()
            .WithNone<ObjectBinder.MainEntityLinkData>()
            .ForEach(
                (Entity ent, int entityInQueryIndex, ref Translation pos, ref Rotation rot, in ObjectInitializeData init) =>
                {

                    pos.Value = init.pos;
                    rot.Value = init.rot;

                    cmd.RemoveComponent<ObjectInitializeData>(entityInQueryIndex, ent);

                }
            )
            .ScheduleParallel();


        //var cmd2 = this.cmdSystem.CreateCommandBuffer().AsParallelWriter();

        var translations = this.GetComponentDataFromEntity<Translation>();
        var rotations = this.GetComponentDataFromEntity<Rotation>();

        this.Entities
            //.WithoutBurst()
            .WithBurst()
            .WithNativeDisableParallelForRestriction(translations)
            .WithNativeDisableParallelForRestriction(rotations)
            .ForEach(
                (Entity ent, int entityInQueryIndex, in ObjectInitializeData init, in ObjectBinder.MainEntityLinkData link) =>
                {

                    var pos = new Translation { Value = init.pos };
                    var rot = new Rotation { Value = init.rot };

                    translations[link.MainEntity] = pos;
                    rotations[link.MainEntity] = rot;

                    cmd.RemoveComponent<ObjectInitializeData>(entityInQueryIndex, ent);

                }
            )
            .ScheduleParallel();
    }

}