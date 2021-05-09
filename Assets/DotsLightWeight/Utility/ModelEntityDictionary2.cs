using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace DotsLite.Model.Authoring
{
    using MeshDictionary = Dictionary<(BoneType bonetype, Mesh mesh), Entity>;

    public class ModelEntityDictionary2 : GameObjectConversionSystem
    {
        public class Data : IComponentData
        {
            public MeshDictionary MeshToEntity;
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

    public static class ModelEntityDictionary2Extension
    {

        public static MeshDictionary GetModelDictionary(this GameObjectConversionSystem gcs)
        {
            if (!gcs.HasSingleton<ModelEntityDictionary2.Data>()) return create_().MeshToEntity;

            return gcs.GetSingleton<ModelEntityDictionary2.Data>().MeshToEntity;


            ModelEntityDictionary2.Data create_()
            {
                var newent = gcs.EntityManager.CreateEntity(typeof(ModelEntityDictionary2.Data));
                var newholder = new ModelEntityDictionary2.Data
                {
                    MeshToEntity = new MeshDictionary(),
                };
                gcs.EntityManager.SetComponentData(newent, newholder);
                return newholder;
            }
        }

        //public static Entity GetOrCreateEntity(this GameObjectConversionSystem gcs, GameObject top, MonoBehaviour key)
        //{
        //    var dict = gcs.GetEntityDictionary();

        //    var isSuccess = dict.TryGetValue(key, out var entity);
        //    if (isSuccess) return entity;

        //    var newentity = gcs.CreateAdditionalEntity(top);
        //    dict[key] = newentity;
        //    return newentity;
        //}

        //public static Entity GetOrCreateEntity(this GameObjectConversionSystem gcs, MonoBehaviour top, MonoBehaviour key)
        //    => gcs.GetOrCreateEntity(top.gameObject, key);

        //public static Entity GetOrCreateEntity(this GameObjectConversionSystem gcs, MonoBehaviour key)
        //    => gcs.GetOrCreateEntity(key, key);

        //public static Entity GetOrCreateEntity
        //    (this GameObjectConversionSystem gcs, MonoBehaviour top, MonoBehaviour key, ComponentTypes types)
        //{
        //    var ent = gcs.GetOrCreateEntity(top, key);
        //    gcs.DstEntityManager.AddComponents(ent, types);
        //    return ent;
        //}

        //public static Entity GetOrCreateEntity(this GameObjectConversionSystem gcs, MonoBehaviour key, ComponentTypes types) =>
        //    gcs.GetOrCreateEntity(key, key, types);
    }
}
