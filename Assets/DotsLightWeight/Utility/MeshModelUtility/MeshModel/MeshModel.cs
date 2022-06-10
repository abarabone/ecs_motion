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
    public class MeshModel : MeshModelBase
    {
        public interface IVertexSelector : IVertexBuilder { }

        [SerializeReference, SubclassSelector]
        public IVertexSelector vtxBuilder;

        protected override IVertexBuilder VtxBuilder => this.vtxBuilder;
    }

    public class MeshModelBase : MonoBehaviour, IMeshModel
    {


        public MeshModelBase() =>
            this.GenerateSourcePrefabKey();

        public void GenerateSourcePrefabKey() =>
            this.sourcePrefabKey.Value = new System.Random(this.GetHashCode()).Next();

        [SerializeField]
        protected SourcePrefabKeyUnit sourcePrefabKey;



        [SerializeField]
        public Shader shader;


        [SerializeField]
        protected BoneType boneType = BoneType.RT;
        [SerializeField]
        protected DrawModel.SortOrder sortOrder = DrawModel.SortOrder.desc;
        
        protected virtual int optionalVectorLength => 0;
        protected virtual int boneLength => 1;


        
        public virtual GameObject Obj => this.gameObject;
        public virtual Transform TfRoot => this.gameObject.transform;
        public virtual IEnumerable<Transform> QueryBones => null;
        public virtual SourcePrefabKeyUnit SourcePrefabKey => this.sourcePrefabKey;

        public virtual IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
            this.gameObject.GetComponentsInChildren<Renderer>()
                .Select(x => x.gameObject)
                .QueryMeshMatsTransform_IfHaving();



        [SerializeReference, SubclassSelector]
        public IIndexSelector idxBuilder;

        protected virtual IIndexBuilder IdxBuilder => this.idxBuilder;
        protected virtual IVertexBuilder VtxBuilder => throw new NotImplementedException();



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
            return () => meshpack.CreateMeshData(md, this.IdxBuilder, this.VtxBuilder, p);
        }

        public virtual Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            return gcs.CreateDrawModelEntityComponents(mesh, mat,
                this.boneType, this.boneLength, this.sortOrder, this.optionalVectorLength);
        }

    }


    [Serializable]
    public class LodMeshModelBase : MeshModelBase, IMeshModelLod
    {

        [SerializeField]
        public float limitDistance;

        [SerializeField]
        public float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }
}