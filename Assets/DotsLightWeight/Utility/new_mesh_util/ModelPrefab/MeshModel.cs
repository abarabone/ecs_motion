﻿//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System;
//using System.Linq;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Transforms;
//using Unity.Mathematics;
//using Unity.Linq;

//namespace DotsLite.Model.Authoring
//{
//    using DotsLite.Geometry;
//    using DotsLite.Geometry.prev;

//    [Serializable]
//    public class PositionNormalUv :
//        MeshModel<UI32, PositionNormalUvVertex>, MeshModelAuthoring1.IMeshModelSelector
//    { }

//    [Serializable]
//    public class PositionNormalUvWithPaletteI32 :
//        MeshWithPaletteModel<UI32, PositionNormalUvWithPaletteVertex>, MeshModelAuthoring1.IMeshModelSelector
//    { }
//}

//namespace DotsLite.Model.Authoring
//{
//    using DotsLite.Draw;
//    using DotsLite.Draw.Authoring;
//    using DotsLite.Geometry;
//    using DotsLite.Structure.Authoring;
//    using DotsLite.Utilities;
//    using DotsLite.Common.Extension;
//    using DotsLite.Misc;

//    public class MeshModel : MonoBehaviour, IMeshModel
//    {

//        [SerializeField]
//        public GameObject objectTop;

//        [SerializeField]
//        public Shader shader;


//        [SerializeField]
//        protected TransformMode tfMode = TransformMode.RT;
//        [SerializeField]
//        protected DrawModel.SortOrder sortOrder = DrawModel.SortOrder.desc;
        
//        protected virtual int optionalVectorLength => 0;
//        protected virtual int boneLength => 1;


//        public virtual GameObject Obj => this.objectTop;
//        public virtual Transform TfRoot => this.objectTop.transform;

//        public virtual IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
//            this.objectTop.GetComponentsInChildren<Renderer>()
//                .Select(x => x.gameObject)
//                .QueryMeshMatsTransform_IfHaving();




//        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
//        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams




//        public virtual Func<IMeshElements> BuildMeshCombiner(
//            SrcMeshesModelCombinePack meshpack,
//            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary)
//        {
//            var sourcePrefabKey = this.GetHashCode();

//            var atlas = atlasDictionary.srckeyToAtlas[sourcePrefabKey].GetHashCode();
//            var texdict = atlasDictionary.texHashToUvRect;
//            var p = this.QueryMmts.calculateParameters(
//                this.TfRoot, this.QueryBones?.ToArray(), subtexhash => texdict[atlas, subtexhash], null);

//            return meshpack.BuildCombiner<TIdx, TVtx>(p);
//        }

//        public virtual Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
//        {
//            var mat = new Material(this.shader);
//            mat.enableInstancing = true;
//            mat.mainTexture = atlas;

//            var boneType = this.tfMode.ToBoneType();
//            return gcs.CreateDrawModelEntityComponents(
//                mesh, mat, boneType,
//                this.boneLength, this.sortOrder, this.optionalVectorLength);
//        }

//    }


//    [Serializable]
//    public class LodMeshModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>, IMeshModelLod
//        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
//        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
//    {

//        [SerializeField]
//        public float limitDistance;

//        [SerializeField]
//        public float margin;


//        public float LimitDistance => this.limitDistance;
//        public float Margin => this.margin;
//    }
//}