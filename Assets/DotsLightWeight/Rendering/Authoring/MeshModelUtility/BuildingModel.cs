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
    public class BuildingModel<TIdx, TVtx> : LodMeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {


        protected override int optionalVectorLength => 4;
        protected override int boneLength => 1;


        public override (SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary)
        {
            var atlas = atlasDictionary.srckeyToAtlas[this.sourcePrefabKey].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var mmts = this.QueryMmts.ToArray();
            var p = mmts.calculateParameters(
                this.TfRoot, this.QueryBones?.ToArray(), part => texdict[atlas, part], null);
            mmts.CalculatePalletSubIndexParameter(ref p);

            return (
                this.sourcePrefabKey,
                meshpack.BuildCombiner<TIdx, TVtx>(p)
            );
        }
    }

}