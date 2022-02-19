using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace DotsLite.Draw.Authoring
{
    using DotsLite.Geometry;

    /// <summary>
    /// 
    /// </summary>
    public class ConvertedMeshDictionary : GameObjectConversionSystem
    {

        public class Data : IComponentData
        {
            public Dictionary<SourcePrefabKeyUnit, Mesh> MeshDictionary;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            //Debug.Log("dic on");

            var dict = new Dictionary<SourcePrefabKeyUnit, Mesh>();
            this.EntityManager.CreateEntity( typeof( Data ) );
            this.SetSingleton( new Data { MeshDictionary = dict } );
        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //Debug.Log("dic off");

            this.GetSingleton<Data>().MeshDictionary.Clear();
            this.GetSingleton<Data>().MeshDictionary = null;
            this.EntityManager.DestroyEntity( this.GetSingletonEntity<Data>() );
        }
    }


    static public class ConvertedMeshDictionaryExtension
    {

        static public Dictionary<SourcePrefabKeyUnit, Mesh> GetMeshDictionary(this GameObjectConversionSystem gcs)
        {
            return gcs.GetSingleton<ConvertedMeshDictionary.Data>().MeshDictionary;
            // texutre atlas と同じにしよう
        }


        static public void AddToMeshDictionary(this GameObjectConversionSystem gcs, SourcePrefabKeyUnit key, Mesh mesh)
        {
            //Debug.Log("addst");
            //Debug.Log(gcs);
            //Debug.Log(gcs.GetSingleton<StructureMeshDictionary.Data>());
            //Debug.Log(gcs.GetSingleton<StructureMeshDictionary.Data>().MeshDictionary);
            gcs.GetSingleton<ConvertedMeshDictionary.Data>().MeshDictionary.Add(key, mesh);
        }

        static public Mesh GetMeshFromDictionary(this GameObjectConversionSystem gcs, SourcePrefabKeyUnit key )
        {
            var isExits = gcs.GetSingleton<ConvertedMeshDictionary.Data>().MeshDictionary
                .TryGetValue(key, out var mesh);

            return mesh;
        }


        static public bool IsExistingInMeshDictionary(this GameObjectConversionSystem gcs, SourcePrefabKeyUnit key)
        {
            return gcs.GetSingleton<ConvertedMeshDictionary.Data>().MeshDictionary
                .ContainsKey(key);
        }
        
    }

}