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
    using DotsLite.Geometry;

    [Serializable]
    public class PositionOnly : PositionVertexBuilder, MeshModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUv : PositionUvVertexBuilder, MeshModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUvNormal : PositionUvNormalVertexBuilder, MeshModel.IVertexSelector
    { }
}

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
    public class MeshModel : MonoBehaviour, IMeshModel
    {

        //public MeshModel() =>
        //    this.GenerateSourcePrefabKey();

        //public void GenerateSourcePrefabKey() =>
        //    this.sourcePrefabKey.Value = new System.Random(this.GetHashCode()).Next();

        //[SerializeField]
        //protected SourcePrefabKeyUnit sourcePrefabKey;

        [SerializeField]
        public GameObject objectTop;

        [SerializeField]
        public Shader shader;


        public interface IVertexSelector : IVertexBuilder { }

        [SerializeReference, SubclassSelector]
        public IIndexSelector idxBuilder;

        [SerializeReference, SubclassSelector]
        public IVertexSelector vtxBuilder;



        [SerializeField]
        protected BoneType boneType = BoneType.RT;
        [SerializeField]
        protected DrawModel.SortOrder sortOrder = DrawModel.SortOrder.desc;
        
        protected virtual int optionalVectorLength => 0;
        protected virtual int boneLength => 1;


        public GameObject AsGameObject => this.gameObject;
        
        public virtual GameObject Obj => this.objectTop;
        public virtual Transform TfRoot => this.objectTop.transform;
        public virtual IEnumerable<Transform> QueryBones => null;
        //public virtual SourcePrefabKeyUnit SourcePrefabKey => this.sourcePrefabKey;

        public virtual IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
            this.objectTop.GetComponentsInChildren<Renderer>()
                .Select(x => x.gameObject)
                .QueryMeshMatsTransform_IfHaving();




        ////public virtual (SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
        ////    SrcMeshesModelCombinePack meshpack, Func<int>)
        ////{
        ////    var atlas = atlasDictionary.srckeyToAtlas[this.sourcePrefabKey].GetHashCode();
        ////    var texdict = atlasDictionary.texHashToUvRect;
        ////    var p = this.QueryMmts.calculateParameters(
        ////        this.TfRoot, this.QueryBones?.ToArray(), subtexhash => texdict[atlas, subtexhash], null);

        ////    return (
        ////        this.sourcePrefabKey,
        ////        meshpack.BuildCombiner<TIdx, TVtx>(p)
        ////    );
        ////}
        public virtual Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<IMeshModel, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var atlas = atlasDictionary.modelToAtlas[this].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var p = this.QueryMmts.calculateParameters(
                this.TfRoot, this.QueryBones?.ToArray(), subtexhash => texdict[atlas, subtexhash], null);

            var md = MeshCreatorUtility.AllocateMeshData();
            return () => meshpack.CreateMeshData(md, this.idxBuilder, this.vtxBuilder, p);
        }

        public virtual void InitModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            var ent = gcs.GetPrimaryEntity(this.AsGameObject);
            gcs.InitDrawModelEntityComponents(ent, mesh, mat,
                this.boneType, this.boneLength, this.sortOrder, this.optionalVectorLength);
        }

    }


    [Serializable]
    public class LodMeshModel : MeshModel, IMeshModelLod
    {

        [SerializeField]
        public float limitDistance;

        [SerializeField]
        public float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }
}