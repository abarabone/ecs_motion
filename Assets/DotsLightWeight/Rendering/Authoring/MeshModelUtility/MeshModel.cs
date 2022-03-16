using System.Collections;
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

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;

    [Serializable]
    public class MeshModel<TIdx, TVtx> : IMeshModel
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        //public MeshModel(GameObject obj, Shader shader)
        //{
        //    this.objectTop = obj;
        //    this.shader = shader;
        //    this.SourcePrefabKey = obj.GetComponent<ModelGroupAuthoring.ModelAuthoringBase>().SourcePrefabKey;
        //}
        public void GenerateSourcePrefabKey() =>
            this.sourcePrefabKey.Value = new System.Random(this.GetHashCode()).Next();

        [SerializeField]
        protected SourcePrefabKeyUnit sourcePrefabKey;

        [SerializeField]
        public GameObject objectTop;

        [SerializeField]
        public Shader shader;


        public virtual GameObject Obj => this.objectTop;
        public virtual Transform TfRoot => this.objectTop.transform;
        public virtual IEnumerable<Transform> QueryBones => null;
        public virtual SourcePrefabKeyUnit SourcePrefabKey => this.sourcePrefabKey;

        public virtual IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
            this.objectTop.GetComponentsInChildren<Renderer>()
                .Select(x => x.gameObject)
                .QueryMeshMatsTransform_IfHaving();


        public virtual Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType BoneType = BoneType.RT;
            const int boneLength = 1;

            return gcs.CreateDrawModelEntityComponents(mesh, mat, BoneType, boneLength, DrawModel.SortOrder.desc);
        }

        public virtual (SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary)
        {
            var atlas = atlasDictionary.srckeyToAtlas[this.sourcePrefabKey].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            return (
                this.sourcePrefabKey,
                meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, this.QueryBones?.ToArray(), part => texdict[atlas, part])
            );
        }
    }


    [Serializable]
    public class LodMeshModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>, IMeshModelLod
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        //public LodMeshModel(GameObject obj, Shader shader) : base(obj, shader)
        //{ }

        [SerializeField]
        public float limitDistance;

        [SerializeField]
        public float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }
}