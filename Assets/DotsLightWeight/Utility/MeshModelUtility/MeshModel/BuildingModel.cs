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

    [Serializable]
    public class BuildingModel : LodMeshModelBase
    {

        protected override int optionalVectorLength => 4;
        protected override int boneLength => 1;


        public override Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<IMeshModel, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var atlas = atlasDictionary.modelToAtlas[this].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var mmts = this.QueryMmts.ToArray();
            var p = mmts.calculateParameters(
                this.Obj.transform, this.QueryBones?.ToArray(), subtexhash => texdict[atlas, subtexhash], null);
            mmts.CalculatePaletteSubIndexParameter(ref p);

            var md = MeshCreatorUtility.AllocateMeshData();
            return () => meshpack.CreateMeshData(md, this.IdxBuilder, this.VtxBuilder, p);
        }


        public interface IVertexSelector : IVertexBuilder { }

        [SerializeReference, SubclassSelector]
        public IVertexSelector vtxBuilder;

        protected override IVertexBuilder VtxBuilder => this.vtxBuilder;
    }

}