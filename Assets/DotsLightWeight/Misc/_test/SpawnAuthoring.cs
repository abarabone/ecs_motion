using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

using DotsLite.Model;
using DotsLite.Model.Authoring;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System;
using DotsLite.Dependency;
using DotsLite.Draw;

public class SpawnAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{

    public ModelGroupAuthoring.ModelAuthoringBase prefab;

    public int3 Length;
    public float3 span;


    public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
    {
        if (!this.isActiveAndEnabled) return;

        referencedPrefabs.Add( this.prefab.gameObject );

    }

    public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
    {
        if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


        var prefab_ent = conversionSystem.GetPrimaryEntity( this.prefab );

        dstManager.AddComponentData(entity, new Spawn.EntryData
        {
            pos = this.transform.position,
            prefab = prefab_ent,
        });
        dstManager.AddComponentData(entity, new Spawn.SpanData
        {
            length = this.Length,
            span = this.span,
        });

    }
    

}

static class Spawn
{
    public struct EntryData : IComponentData
    {
        public float3 pos;
        public quaternion rot;
        public Entity prefab;
        public int paletteIndex;
    }
    public struct SpanData : IComponentData
    {
        public float3 span;
        public int3 length;
        public int i;
    }
}

/// <summary>
/// spawn entity の EntryData と SpanData を列挙し、指定の個数分だけ、プレハブからインスタンスを生成する。
/// spawn entity は、インスタンス生成後に破棄される。
/// インスタンスは、ObjectInitializeData によって初期化される。（直接じゃダメなのか？←ＴＦの違いを吸収するためか？）
/// </summary>
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ObjectInitializeSystem))]
public partial class SpawnFreqencySystem : DependencyAccessableSystemBase
{


    CommandBufferDependency.Sender cmddep;


    protected override void OnCreate()
    {
        base.OnCreate();

        this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
    }

    protected override void OnUpdate()
    {
        using var cmdScope = this.cmddep.WithDependencyScope();


        var cmd = cmdScope.CommandBuffer.AsParallelWriter();

        this.Entities
            .WithBurst()
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, ref Spawn.SpanData span, in Spawn.EntryData entry) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, entry.prefab);

                    var i = span.i;
                    var l = span.length;
                    var s = span.span;

                    cmd.AddComponent(entityInQueryIndex, ent, new ObjectInitializeData
                    {
                        pos = entry.pos + new float3(i % l.x * s.x, i / l.x % l.y * s.y, i / (l.x * l.y) * s.z),
                        rot = math.any(entry.rot.value) ? entry.rot : quaternion.identity,
                    });

                    if (++span.i >= l.x * l.y * l.z)
                        cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();
    }

}

/// <summary>
/// spawn entity の EntryData を列挙し、プレハブからインスタンスを生成する。
/// spawn entity は、インスタンス生成後に破棄される。
/// インスタンスは、ObjectInitializeData によって初期化される。（直接じゃダメなのか？←ＴＦの違いを吸収するためか？）
/// </summary>
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ObjectInitializeSystem))]
public partial class SpawnSystem : DependencyAccessableSystemBase
{


    CommandBufferDependency.Sender cmddep;


    protected override void OnCreate()
    {
        base.OnCreate();

        this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
    }

    protected override void OnUpdate()
    {
        using var cmdScope = this.cmddep.WithDependencyScope();


        var cmd = cmdScope.CommandBuffer.AsParallelWriter();

        this.Entities
            .WithBurst()
            .WithNone<Spawn.SpanData>()
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, in Spawn.EntryData entry) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, entry.prefab);

                    cmd.AddComponent(entityInQueryIndex, ent, new ObjectInitializeData
                    {
                        pos = entry.pos,
                        rot = math.any(entry.rot.value) ? entry.rot : quaternion.identity,
                    });

                    if (entry.paletteIndex != -1)// コンポーネントを独立させたほうがいいよな…
                    {
                        cmd.AddComponent(entityInQueryIndex, ent, new Palette.ColorPaletteData
                        {
                            BaseIndex = entry.paletteIndex,
                        });
                        cmd.AddComponent(entityInQueryIndex, ent, new DrawInstance.TransferSpecialTag { });
                    }

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
public partial class ObjectInitializeSystem : DependencyAccessableSystemBase
{


    CommandBufferDependency.Sender cmddep;


    protected override void OnCreate()
    {
        base.OnCreate();

        this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
    }

    protected override void OnUpdate()
    {
        using var cmdScope = this.cmddep.WithDependencyScope();


        var cmd = cmdScope.CommandBuffer.AsParallelWriter();

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