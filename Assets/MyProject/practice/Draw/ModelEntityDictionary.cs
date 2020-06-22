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
    /// モデルプレハブはモデルのバインダーエンティティに紐づけるので、別枠が必要になってしまった…。
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

            this.GetSingleton<Data>().ModelDictionary = null;
            this.EntityManager.DestroyEntity( this.GetSingletonEntity<Data>() );
        }
    }

}