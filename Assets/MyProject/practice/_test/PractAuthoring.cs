using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

using Abarabone.Model;


public struct SpawnData : IComponentData
{
    public Entity ent;
    public int i;
}

public class PractAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{

    public GameObject prefab;

    Entity prefabEntity;


    void IDeclareReferencedPrefabs.DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
    {

        referencedPrefabs.Add( this.prefab );

    }

    void IConvertGameObjectToEntity.Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
    {

        var prefab_ent = conversionSystem.GetPrimaryEntity( this.prefab );

        dstManager.AddComponentData( entity, new SpawnData { ent = prefab_ent } );
        
    }
    

}

public class PracSpawnSystem : SystemBase
{

    protected override void OnStartRunning()
    {
        //var spawn = this.GetSingleton<SpawnData>();
        //this.EntityManager.Instantiate( spawn.ent );
    }

    protected override void OnUpdate()
    {
        //if( spawn.i++ == 0 ) this.EntityManager.DestroyEntity( this.GetSingletonEntity<SpawnData>() );
        //this.SetSingleton<SpawnData>( spawn );

        //this.Entities
        //    .WithoutBurst()
        //    .ForEach(
        //        ( Entity ent, ref SpawnData spawn ) =>
        //        {
        //            this.EntityManager.Instantiate( spawn.ent );
        //            this.EntityManager.DestroyEntity( ent );
        //        }
        //    )
        //    .Run();
    }

}