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

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw.Authoring;
    using DotsLite.Character;
    using DotsLite.Common.Extension;
    using DotsLite.CharacterMotion.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Draw;

    [Serializable]
    public class CharacterModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        //public CharacterModel(GameObject obj, Shader shader, Transform boneTop) : base(obj, shader)
        //{
        //    this.BoneTop = boneTop;
        //}

        [HideInInspector]
        public Transform boneTop;


        public override Transform TfRoot => this.objectTop.Children().First().transform;// これでいいのか？

        public override IEnumerable<Transform> QueryBones => this.boneTop.gameObject.DescendantsAndSelf()
            .Where(x => !x.name.StartsWith("_"))
            .Select(x => x.transform);



        public override Entity CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType BoneType = BoneType.RT;
            var boneLength = this.QueryBones.Count();

            return gcs.CreateDrawModelEntityComponents(
                mesh, mat, BoneType, boneLength, DrawModel.SortOrder.desc);
        }

    }

    //[Serializable]
    //public class CharacterModel<TIdx, TVtx> : IMeshModel, IMeshModelLod
    //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
    //    where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    //{
    //    public CharacterModel(GameObject obj, Shader shader, Transform boneTop)
    //    {
    //        this.objectTop = obj;
    //        this.shader = shader;
    //        this.BoneTop = boneTop;
    //    }

    //    [SerializeField]
    //    GameObject objectTop;

    //    [SerializeField]
    //    float limitDistance;
    //    [SerializeField]
    //    float margin;


    //    [SerializeField]
    //    Shader shader;

    //    public Transform BoneTop;


    //    public GameObject Obj => this.objectTop;
    //    public Transform TfRoot => this.objectTop.Children().First().transform;// これでいいのか？
    //    public int SourcePrefabKey { get; }

    //    Transform[] _bones = null;
    //    public Transform[] Bones => this._bones ??= this.BoneTop.gameObject.DescendantsAndSelf()
    //        .Where(x => !x.name.StartsWith("_"))
    //        .Select(x => x.transform)
    //        .ToArray();

    //    public float LimitDistance => this.limitDistance;
    //    public float Margin => this.margin;




    //    public IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
    //        this.objectTop.GetComponentsInChildren<SkinnedMeshRenderer>()
    //            .Select(x => x.gameObject)
    //            .QueryMeshMatsTransform_IfHaving();



    //    public void CreateModelEntity
    //        (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
    //    {
    //        var mat = new Material(this.shader);
    //        mat.enableInstancing = true;
    //        mat.mainTexture = atlas;

    //        const BoneType BoneType = BoneType.RT;
    //        var boneLength = Bones.Length;

    //        gcs.CreateDrawModelEntityComponents(this.Obj, mesh, mat, BoneType, boneLength, DrawModel.SortOrder.desc);
    //    }

    //    public (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner
    //        (
    //            SrcMeshesModelCombinePack meshpack,
    //            Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
    //        )
    //    {
    //        var atlas = atlasDictionary.objectToAtlas[this.Obj].GetHashCode();
    //        var texdict = atlasDictionary.texHashToUvRect;
    //        return (
    //            this.Obj,
    //            meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, part => texdict[atlas, part], this.Bones)
    //        );
    //    }
    //}


}

