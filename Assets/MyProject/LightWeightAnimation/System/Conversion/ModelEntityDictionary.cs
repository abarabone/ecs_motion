using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Model.Authoring
{

    /// <summary>
    /// モデルエンティティとゲームオブジェクトを紐づける仕組み。
    /// モデルプレハブはモデルのメインエンティティに紐づけるので、別枠が必要になった。（そもそもインスタンスとリソースは別か）
    /// </summary>
    public class ModelEntityDictionary : GameObjectConversionSystem
    {

        public class Data : IComponentData
        {
            public Dictionary<GameObject, Entity> ModelDictionary;
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            var dict = new Dictionary<GameObject, Entity>();
            this.EntityManager.CreateEntity( typeof( Data ) );
            this.SetSingleton( new Data { ModelDictionary = dict } );
        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.GetSingleton<Data>().ModelDictionary.Clear();
            this.GetSingleton<Data>().ModelDictionary = null;
            this.EntityManager.DestroyEntity( this.GetSingletonEntity<Data>() );
        }
    }


    static public class ModelEntityDictionaryExtension
    {
        static public void AddToModelEntityDictionary
            ( this GameObjectConversionSystem gcs, GameObject topGameObject, Entity entity ) =>
            gcs.GetSingleton<ModelEntityDictionary.Data>().ModelDictionary.Add(topGameObject, entity);

        static public Entity GetFromModelEntityDictionary
            (this GameObjectConversionSystem gcs, GameObject topGameObject)
        {
            gcs.GetSingleton<ModelEntityDictionary.Data>().ModelDictionary.TryGetValue(topGameObject, out var entity);

            return entity;
        }

        static public bool IsExistsInModelEntityDictionary
            (this GameObjectConversionSystem gcs, GameObject topGameObject) =>
            gcs.GetSingleton<ModelEntityDictionary.Data>().ModelDictionary.ContainsKey(topGameObject);
    }

}