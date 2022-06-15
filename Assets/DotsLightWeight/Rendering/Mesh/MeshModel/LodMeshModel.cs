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

namespace DotsLite.Model.Authoring.Vertex.LodMeshModel
{
    using DotsLite.Geometry;

    [Serializable]
    public class PositionOnly : PositionVertexBuilder, Authoring.LodMeshModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUv : PositionUvVertexBuilder, Authoring.LodMeshModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUvNormal : PositionUvNormalVertexBuilder, Authoring.LodMeshModel.IVertexSelector
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
    public class LodMeshModel : LodMeshModelBase
    {
        public interface IVertexSelector : IVertexBuilder { }


        [SerializeReference, SubclassSelector]
        public IIndexSelector idxBuilder;
        protected override IIndexBuilder IdxBuilder => this.idxBuilder;

        [SerializeReference, SubclassSelector]
        public IVertexSelector vtxBuilder;
        protected override IVertexBuilder VtxBuilder => this.vtxBuilder;
    }
}