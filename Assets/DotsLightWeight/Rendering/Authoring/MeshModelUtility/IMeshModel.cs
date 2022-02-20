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

    [Serializable]
    public struct SourcePrefabKeyUnit
    {
        public int Value;
    }


    public interface IMeshModel
    {
        SourcePrefabKeyUnit SourcePrefabKey { get; }
        void GenerateSourcePrefabKey();

        GameObject Obj { get; }
        Transform TfRoot { get; }

        IEnumerable<Transform> QueryBones { get; }
        IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts { get; }
        

        Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas);


        (SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary);
    }


    public interface IMeshModelLod : IMeshModel
    {
        float LimitDistance { get; }
        float Margin { get; }
    }

}
