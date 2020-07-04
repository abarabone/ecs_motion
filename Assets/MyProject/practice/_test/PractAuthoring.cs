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

public struct SpawnData : IComponentData
{
    public Entity ent;
    public int i;
}

public class PractAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{

    public ModelGroupAuthoring.ModelAuthoringBase prefab;

    public int num;

    Entity prefabEntity;


    void IDeclareReferencedPrefabs.DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
    {

        referencedPrefabs.Add( this.prefab.gameObject );

    }

    void IConvertGameObjectToEntity.Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
    {

        var prefab_ent = conversionSystem.GetPrimaryEntity( this.prefab );

        dstManager.AddComponentData( entity, new SpawnData { ent = prefab_ent, i = this.num } );
        
    }
    

}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class PracSpawnSystem : SystemBase
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
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, ref SpawnData spawn) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, spawn.ent);

                    cmd.AddComponent(entityInQueryIndex, ent,
                        new ObjectInitializeData {pos = new float3(spawn.i%20,spawn.i/20,0.0f) }
                    );

                    if( --spawn.i == 0 )
                        cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();
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
        this.cmdSystem = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
    }


    protected override void OnUpdate()
    {
        var cmd = this.cmdSystem.CreateCommandBuffer().ToConcurrent();

        this.Entities
            .ForEach(
                (Entity ent, int entityInQueryIndex, ref Translation pos, in ObjectInitializeData init) =>
                {

                    pos.Value = init.pos;

                    cmd.RemoveComponent<ObjectInitializeData>(entityInQueryIndex, ent);

                }
            )
            .ScheduleParallel();


        var translations = this.GetComponentDataFromEntity<Translation>();

        this.Entities
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
    }

}