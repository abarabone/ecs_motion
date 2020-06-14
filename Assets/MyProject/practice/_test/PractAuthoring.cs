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

        //this.prefabEntity = dstManager.GetComponentData<ModelPrefabHeadData>( prefab_ent ).PrefabHeadEntity;
        Debug.Log( this.name );
    }


    int i;
    private void Update()
    {
        if( i++ > 3 ) return;

        var em = World.DefaultGameObjectInjectionWorld.EntityManager;

        var ent = em.Instantiate( this.prefabEntity );
        em.SetComponentData( ent, new Translation { Value = new float3( 0, i, 0 ) } );
    }

}
