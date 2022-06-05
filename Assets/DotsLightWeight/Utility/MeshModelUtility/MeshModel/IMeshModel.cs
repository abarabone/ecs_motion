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

    //[Serializable]
    //public struct SourcePrefabKeyUnit
    //{
    //    public int Value;
    //}


    //public enum TransformMode
    //{
    //    T               = 1 << 8 | Model.BoneType.T,
    //    RT              = 2 << 8 | Model.BoneType.RT,
    //    RTS             = 3 << 8 | Model.BoneType.RTS,
    //    ST              = 4 << 8 | Model.BoneType.ST,
    //    SRT             = 5 << 8 | Model.BoneType.SRT,
    //    Matrix4x3       = 6 << 8 | Model.BoneType.Matrix4x3,
    //    PtoP            = 7 << 8 | Model.BoneType.PtoP,
    //    P1uv            = 8 << 8 | Model.BoneType.P1uv,
    //    PtoPuv          = 9 << 8 | Model.BoneType.PtoPuv,
    //}
    //public static class TransformModeUtility
    //{
    //    public static Model.BoneType ToBoneType(this TransformMode tfmode) =>
    //        (Model.BoneType)((int)tfmode & 0xff);
    //}

    public interface IMeshModel
    {
        //SourcePrefabKeyUnit SourcePrefabKey { get; }
        //void GenerateSourcePrefabKey();

        GameObject AsGameObject { get; }

        GameObject Obj { get; }// Ç±ÇÍÇ¢ÇÈÇÒÇæÇ¡ÇØÅH
        Transform TfRoot { get; }

        IEnumerable<Transform> QueryBones { get; }
        IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts { get; }


        //(SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
        //    SrcMeshesModelCombinePack meshpack,
        //    Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary);

        //(SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
        //    GameObjectConversionSystem gcs, SrcMeshesModelCombinePack meshpack);

        Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<IMeshModel, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary);


        void InitModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas);

    }


    public interface IMeshModelLod : IMeshModel
    {
        float LimitDistance { get; }
        float Margin { get; }
    }

}
