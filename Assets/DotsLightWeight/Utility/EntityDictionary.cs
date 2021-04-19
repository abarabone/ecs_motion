using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Abarabone.Common.Extension
{

    public class EntityDictionary : GameObjectConversionSystem
    {
        public class Data : IComponentData
        {
            public Dictionary<MonoBehaviour, Entity> ComponentToEntity;
        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (this.HasSingleton<Data>())
            {
                this.EntityManager.DestroyEntity(this.GetSingletonEntity<Data>());
            }
        }
    }

    public static class EntityDictionaryExtension
    {

        public static Dictionary<MonoBehaviour, Entity> GetEntityDictionary(this GameObjectConversionSystem gcs)
        {
            if (!gcs.HasSingleton<EntityDictionary.Data>()) return create_().ComponentToEntity;

            return gcs.GetSingleton<EntityDictionary.Data>().ComponentToEntity;


            EntityDictionary.Data create_()
            {
                var newent = gcs.EntityManager.CreateEntity(typeof(EntityDictionary.Data));
                var newholder = new EntityDictionary.Data
                {
                    ComponentToEntity = new Dictionary<MonoBehaviour, Entity>(),
                };
                gcs.EntityManager.SetComponentData(newent, newholder);
                return newholder;
            }
        }

    }
}
