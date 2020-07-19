using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Structure.Authoring
{

    /// <summary>
    /// モデルエンティティとゲームオブジェクトを紐づける仕組み。
    /// モデルプレハブはモデルのメインエンティティに紐づけるので、別枠が必要になった。（そもそもインスタンスとリソースは別か）
    /// </summary>
    public class StructureMeshDictionary : GameObjectConversionSystem
    {

        public class Data : IComponentData
        {
            public Dictionary<GameObject, Mesh> MeshDictionary;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            Debug.Log("dic on");

            var dict = new Dictionary<GameObject, Mesh>();
            this.EntityManager.CreateEntity( typeof( Data ) );
            this.SetSingleton( new Data { MeshDictionary = dict } );
        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log("dic off");

            this.GetSingleton<Data>().MeshDictionary.Clear();
            this.GetSingleton<Data>().MeshDictionary = null;
            this.EntityManager.DestroyEntity( this.GetSingletonEntity<Data>() );
        }
    }


    static public class StructureMeshDictionaryExtension
    {
        static public void AddToStructureMeshDictionary
            ( this GameObjectConversionSystem gcs, GameObject topGameObject, Mesh mesh)
        {
            //Debug.Log("addst");
            //Debug.Log(gcs);
            //Debug.Log(gcs.GetSingleton<StructureMeshDictionary.Data>());
            //Debug.Log(gcs.GetSingleton<StructureMeshDictionary.Data>().MeshDictionary);
            gcs.GetSingleton<StructureMeshDictionary.Data>().MeshDictionary.Add(topGameObject, mesh);
        }

        static public Mesh GetFromStructureMeshDictionary
            ( this GameObjectConversionSystem gcs, GameObject topGameObject ) =>
            gcs.GetSingleton<StructureMeshDictionary.Data>().MeshDictionary[topGameObject];
    }

}