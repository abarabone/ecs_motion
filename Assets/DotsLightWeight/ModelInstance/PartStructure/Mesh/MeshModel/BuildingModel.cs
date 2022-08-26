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

namespace DotsLite.Structure.Authoring.Vertex.BuildingModel
{
    using DotsLite.Geometry;

    [Serializable]
    public class PositionNormalUv :
        StructureVertexBuilder, Authoring.BuildingModel.IVertexSelector
    { }
}

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Model.Authoring;
    using DotsLite.Geometry.Palette;
    using DotsLite.Geometry.inner.palette;

    [Serializable]
    public class BuildingModel : LodMeshModelBase
    {

        protected override int optionalVectorLength => 4;
        protected override int boneLength => 1;


        public override Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var p = new AdditionalParameters();
            var atlas = atlasDictionary.modelToAtlas[this].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var mmts = this.QueryMmts.ToArray();
            p.calculateParameters(mmts, this.Obj.transform, subtexhash => texdict[atlas, subtexhash]);
            p.calculatePartParameters(mmts);
            p.CalculatePaletteSubIndexParameter(mmts);

            var md = MeshCreatorUtility.AllocateMeshData();
            return () => meshpack.CreateMeshData(md, this.IdxBuilder, this.VtxBuilder, p);
        }


        public interface IVertexSelector : IVertexBuilder { }


        [SerializeReference, SubclassSelector]
        public IIndexSelector idxBuilder;
        protected override IIndexBuilder IdxBuilder => this.idxBuilder;

        [SerializeReference, SubclassSelector]
        public IVertexSelector vtxBuilder;
        protected override IVertexBuilder VtxBuilder => this.vtxBuilder;
    }

}