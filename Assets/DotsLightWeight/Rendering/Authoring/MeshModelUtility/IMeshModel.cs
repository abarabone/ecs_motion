using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Geometry
{

    public interface IMeshModel
    {
        int SourcePrefabKey { get; }
        void GenerateSourcePrefabKey();

        GameObject Obj { get; }
        Transform TfRoot { get; }

        IEnumerable<Transform> QueryBones { get; }
        IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts { get; }
        

        void CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas);


        (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary);
    }


    public interface IMeshModelLod : IMeshModel
    {
        float LimitDistance { get; }
        float Margin { get; }
    }

}
