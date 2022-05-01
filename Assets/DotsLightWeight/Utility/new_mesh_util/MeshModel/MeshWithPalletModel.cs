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
    public class MeshWithPalletModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {


        public override Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            var boneType = this.tfMode.ToBoneType();
            return gcs.CreateDrawModelEntityComponents(
                mesh, mat, boneType,
                this.boneLength, this.sortOrder, this.optionalVectorLength);
        }

        public override (SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary)
        {
            var atlas = atlasDictionary.srckeyToAtlas[this.sourcePrefabKey].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var p = this.QueryMmts.calculateParameters(
                this.TfRoot, this.QueryBones?.ToArray(), subtexhash => texdict[atlas, subtexhash], null);

            return (
                this.sourcePrefabKey,
                meshpack.BuildCombiner<TIdx, TVtx>(p)
            );
        }
    }


    [Serializable]
    public class LodMeshWithPalletModel<TIdx, TVtx> : MeshWithPalletModel<TIdx, TVtx>, IMeshModelLod
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {

        [SerializeField]
        public float limitDistance;

        [SerializeField]
        public float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }
}