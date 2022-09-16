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

    public abstract class MeshModelBase : MonoBehaviour, IMeshModel
    {


        [SerializeField]
        protected SourcePrefabKeyUnit sourcePrefabKey;
        public SourcePrefabKeyUnit SourcePrefabKey => this.sourcePrefabKey;

        public void GenerateSourcePrefabKey() =>
            this.sourcePrefabKey.Value = new System.Random(this.GetHashCode()).Next();

        void Reset() =>
            this.GenerateSourcePrefabKey();



        [SerializeField]
        public Shader shader;


        [SerializeField]
        protected BoneType boneType = BoneType.RT;
        [SerializeField]
        protected DrawModel.SortOrder sortOrder = DrawModel.SortOrder.desc;


        //protected abstract IIndexBuilder IdxBuilder { get; }
        //protected abstract IVertexBuilder VtxBuilder { get; }


        protected virtual int optionalVectorLength => 0;
        protected virtual int boneLength => 1;



        public GameObject Obj => this.gameObject;

        public virtual IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts =>
            this.gameObject.GetComponentsInChildren<Renderer>()
                .Select(x => x.gameObject)
                .QueryMeshMatsTransform_IfHaving();




        public virtual Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var p = new AdditionalParameters();
            var atlas = atlasDictionary.modelToAtlas[this].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var mmts = this.QueryMmts.ToArray();
            p.calculateParameters(mmts, this.Obj.transform, subtexhash => texdict[atlas, subtexhash]);

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
    public abstract class LodMeshModelBase : MeshModelBase, IMeshModelLod
    {

        [SerializeField]
        public float limitDistance;

        [SerializeField]
        public float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }
}