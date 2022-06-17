using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;

    public class ModelAlias : MonoBehaviour, IMeshModel
    {

        public MeshModelBase LinkToMeshModel;



        public SourcePrefabKeyUnit SourcePrefabKey => this.LinkToMeshModel.SourcePrefabKey;
        public void GenerateSourcePrefabKey() => this.LinkToMeshModel.GenerateSourcePrefabKey();

        public GameObject Obj => this.LinkToMeshModel.Obj;


        public IEnumerable<Transform> QueryBones => this.LinkToMeshModel.QueryBones;
        public IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts => this.LinkToMeshModel.QueryMmts;


        public Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<IMeshModel, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary) => this.LinkToMeshModel.BuildMeshCombiner(meshpack, meshDictionary, atlasDictionary);


        public Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas) =>
            this.LinkToMeshModel.CreateModelEntity(gcs, mesh, atlas);
    }
}
