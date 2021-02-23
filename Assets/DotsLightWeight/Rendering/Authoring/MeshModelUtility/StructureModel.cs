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

namespace Abarabone.Structure.Aurthoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Structure.Authoring;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Misc;

    [Serializable]
    public class StructureModel<TIdx, TVtx> : IMeshModelLod
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        public StructureModel(GameObject obj, Shader shader)
        {
            this.objectTop = obj;
            this.shader = shader;
        }

        [SerializeField]
        GameObject objectTop;

        [SerializeField]
        float limitDistance;
        [SerializeField]
        float margin;


        [SerializeField]
        Shader shader;


        public GameObject Obj => this.objectTop;
        public Transform TfRoot => this.objectTop.transform;
        public Transform[] Bones => null;
        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;





        public IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
            this.objectTop.GetComponentsInChildren<Renderer>()
                .Select(x => x.gameObject)
                .QueryMeshMatsTransform_IfHaving();




        public void CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType boneType = BoneType.TR;
            const int boneLength = 1;
            const int vectorOffsetPerInstance = 4;

            gcs.CreateDrawModelEntityComponents
                (this.Obj, mesh, mat, boneType, boneLength, vectorOffsetPerInstance);
        }

        public (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner
            (
                SrcMeshesModelCombinePack meshpack,
                Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
            )
        {
            var atlas = atlasDictionary.objectToAtlas[this.Obj].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            return (
                this.Obj,
                meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, part => texdict[atlas, part], this.Bones)
            );
        }
    }

}