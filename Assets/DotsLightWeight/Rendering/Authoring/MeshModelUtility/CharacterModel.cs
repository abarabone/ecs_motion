﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Model.Authoring
{
    using Abarabone.Draw.Authoring;
    using Abarabone.Character;
    using Abarabone.Common.Extension;
    using Abarabone.CharacterMotion.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Utilities;

    [Serializable]
    public class CharacterModel<TIdx, TVtx> : IMeshModel, IMeshModelLod
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        public CharacterModel(GameObject obj, Shader shader, Transform boneTop)
        {
            this.objectTop = obj;
            this.shader = shader;
            this.BoneTop = boneTop;
        }

        [SerializeField]
        GameObject objectTop;

        [SerializeField]
        float limitDistance;
        [SerializeField]
        float margin;


        [SerializeField]
        Shader shader;

        public Transform BoneTop;


        public GameObject Obj => this.objectTop;
        public Transform TfRoot => this.objectTop.Children().First().transform;// これでいいのか？
        public Transform[] Bones => this.BoneTop.gameObject.DescendantsAndSelf()
            .Where(x => !x.name.StartsWith("_"))
            .Select(x => x.transform)
            .ToArray();
        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;




        public IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
            this.objectTop.GetComponentsInChildren<SkinnedMeshRenderer>()
                .Select(x => x.gameObject)
                .QueryMeshMatsTransform_IfHaving();



        Transform[] _bones = null;

        public void CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType BoneType = BoneType.TR;
            var boneLength = Bones.Length;

            gcs.CreateDrawModelEntityComponents(this.Obj, mesh, mat, BoneType, boneLength);
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

