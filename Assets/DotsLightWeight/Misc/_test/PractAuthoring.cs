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
using Abarabone.Dependency;

public class PractAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{

    public ModelGroupAuthoring.ModelAuthoringBase prefab;

    public int num;

    //Entity prefabEntity;


    public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
    {

        referencedPrefabs.Add( this.prefab.gameObject );

    }

    public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
    {

        var prefab_ent = conversionSystem.GetPrimaryEntity( this.prefab );

        dstManager.AddComponentData( entity, new SingleSpawnData { ent = prefab_ent, i = this.num } );
        
    }
    

}

public struct SingleSpawnData : IComponentData
{
    public Entity ent;
    public int i;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(ObjectInitializeSystem))]
public class PracSpawnSystem : DependencyAccessableSystemBase
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
            .ForEach(
                (Entity spawnEntity, int entityInQueryIndex, ref SingleSpawnData spawn) =>
                {
                    var ent = cmd.Instantiate(entityInQueryIndex, spawn.ent);

                    cmd.AddComponent(entityInQueryIndex, ent,
                        new ObjectInitializeData { pos = new float3(spawn.i % 20, spawn.i / 20, 0.0f), rot = quaternion.identity }
                        //new ObjectInitializeData { pos = new float3(0,2,0) }
                    );

                    if (--spawn.i == 0)
                        cmd.DestroyEntity(entityInQueryIndex, spawnEntity);
                }
            )
            .ScheduleParallel();
    }

}
