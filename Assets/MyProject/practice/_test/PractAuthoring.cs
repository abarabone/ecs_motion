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

    protected override void OnUpdate()
    {
        if( !this.HasSingleton<SpawnData>() ) return;


        var em = this.EntityManager;

        var spawn = this.GetSingleton<SpawnData>();

        var ent = em.Instantiate( spawn.ent );
        if( !em.HasComponent<Translation>( ent ) ) ent = em.GetComponentData<ModelBinderLinkData>(ent).MainEntity;
        
        em.SetComponentData( ent, new Translation { Value = new float3(0,spawn.i,0) } );

        spawn.i++;

        this.SetSingleton( spawn );

        if( spawn.i >= 1 )
            this.EntityManager.DestroyEntity( this.GetSingletonEntity<SpawnData>() );
    }

}