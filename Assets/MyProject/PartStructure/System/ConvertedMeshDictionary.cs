using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Draw.Authoring
{

    /// <summary>
    /// モデルエンティティとゲームオブジェクトを紐づける仕組み。
    /// モデルプレハブはモデルのトップエンティティに紐づけるので、別枠が必要になった。（そもそもインスタンスとリソースは別か）
    /// </summary>
    public class ConvertedMeshDictionary : GameObjectConversionSystem
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


    static public class ConvertedMeshDictionaryExtension
    {
        static public void AddToMeshDictionary
            ( this GameObjectConversionSystem gcs, GameObject topGameObject, Mesh mesh)
        {
            //Debug.Log("addst");
            //Debug.Log(gcs);
            //Debug.Log(gcs.GetSingleton<StructureMeshDictionary.Data>());
            //Debug.Log(gcs.GetSingleton<StructureMeshDictionary.Data>().MeshDictionary);
            gcs.GetSingleton<ConvertedMeshDictionary.Data>().MeshDictionary.Add(topGameObject, mesh);
        }

        static public Mesh GetFromMeshDictionary
            ( this GameObjectConversionSystem gcs, GameObject topGameObject )
        {
            var isExits = gcs.GetSingleton<ConvertedMeshDictionary.Data>().MeshDictionary
                .TryGetValue(topGameObject, out var mesh);

            return mesh;
        }


        static public bool IsExistingInMeshDictionary
            (this GameObjectConversionSystem gcs, GameObject topGameObject)
        {
            return gcs.GetSingleton<ConvertedMeshDictionary.Data>().MeshDictionary
                .ContainsKey(topGameObject);
        }
        
    }

}