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

public struct SpawnData : IComponentData
{
    public Entity ent;
    public int i;
}

public class PractAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{

    public ModelGroupAuthoring.ModelAuthoringBase prefab;

    Entity prefabEntity;


    void IDeclareReferencedPrefabs.DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
    {

        referencedPrefabs.Add( this.prefab.gameObject );

    }

    void IConvertGameObjectToEntity.Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
    {

        var prefab_ent = conversionSystem.GetPrimaryEntity( this.prefab );

        dstManager.AddComponentData( entity, new SpawnData { ent = prefab_ent } );
        
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

        var binders = this.GetComponentDataFromEntity<BinderObjectMainEntityLinkData>( isReadOnly: true );
        var translations = this.GetComponentDataFromEntity<Translation>();


        this.Entities
            .WithNativeDisableParallelForRestriction(translations)
            .WithReadOnly(binders)
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, ref SpawnData spawn) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, spawn.ent);

                    if (!translations.Exists(ent))
                        ent = binders[ent].MainEntity;


                    //translations[ent] = new Translation { Value = new float3(0, spawn.i, 0) };


                    if (spawn.i++ > 10)
                        cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();
    }

}