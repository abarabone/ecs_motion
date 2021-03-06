﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

namespace Abarabone.Model.Authoring
{
    using Abarabone.Draw;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Structure.Authoring;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Misc;

    [Serializable]
    public class MeshModel<TIdx, TVtx> : IMeshModel
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        public MeshModel(GameObject obj, Shader shader)
        {
            this.objectTop = obj;
            this.shader = shader;
        }

        [SerializeField]
        protected GameObject objectTop;

        [SerializeField]
        protected Shader shader;


        public virtual GameObject Obj => this.objectTop;
        public virtual Transform TfRoot => this.objectTop.transform;
        public virtual Transform[] Bones => null;

        public virtual IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
            this.objectTop.GetComponentsInChildren<Renderer>()
                .Select(x => x.gameObject)
                .QueryMeshMatsTransform_IfHaving();


        public virtual void CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType BoneType = BoneType.TR;
            const int boneLength = 1;

            gcs.CreateDrawModelEntityComponents(this.Obj, mesh, mat, BoneType, boneLength);
        }

        public virtual (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner
            (
                SrcMeshesModelCombinePack meshpack,
                Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
            )
        {
            var atlas = atlasDictionary.objectToAtlas[this.objectTop].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            return (
                this.objectTop,
                meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, part => texdict[atlas, part], this.Bones)
            );
        }
    }


    [Serializable]
    public class LodMeshModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>, IMeshModelLod
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        public LodMeshModel(GameObject obj, Shader shader) : base(obj, shader)
        { }

        [SerializeField]
        float limitDistance;

        [SerializeField]
        float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }
}