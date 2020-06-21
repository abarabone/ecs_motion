using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Authoring
{
    using Draw;
    using Utilities;


    public class ModelGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        public class ModelAuthoringBase : MonoBehaviour
        { }

        public ModelAuthoringBase[] ModelPrefabs;



        void IDeclareReferencedPrefabs.DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.AddRange( this.ModelPrefabs.Select(x => x.gameObject) );
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            dstManager.DestroyEntity( entity );


            prefabEntities = this.ModelPrefabs
                .Select( x => conversionSystem.GetPrimaryEntity( x.gameObject ) )
                .ToArray();

        }

        IEnumerable<Entity> prefabEntities;
        void OnDestroy()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            this.prefabEntities
                .Where( x => em.HasComponent<ModelPrefabNoNeedLinkedEntityGroupTag>(x) )
                .ForEach(
                    x =>
                    {
                        em.RemoveComponent<LinkedEntityGroup>( x );
                        em.RemoveComponent<ModelPrefabNoNeedLinkedEntityGroupTag>( x );
                    }
                );
        }

    }


}
