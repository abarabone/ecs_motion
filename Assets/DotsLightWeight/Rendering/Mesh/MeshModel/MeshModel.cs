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

namespace DotsLite.Model.Authoring.Vertex.MeshModel
{
    using DotsLite.Geometry;

    [Serializable]
    public class PositionOnly : PositionVertexBuilder, Authoring.MeshModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUv : PositionUvVertexBuilder, Authoring.MeshModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUvNormal : PositionUvNormalVertexBuilder, Authoring.MeshModel.IVertexSelector
    { }


    //[Serializable]
    //public class PositionOnlyWithPallet : PositionWithPalletVertexBuilder, Authoring.MeshModel.IVertexSelector
    //{ }

    //[Serializable]
    //public class PositionUvWithPallet : PositionUvWithPalletVertexBuilder, Authoring.MeshModel.IVertexSelector
    //{ }

    [Serializable]
    public class PositionUvNormalWithPalette : PositionUvNormalWithPaletteVertexBuilder, Authoring.MeshModel.IVertexSelector
    { }

}

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;

    [Serializable]
    public class MeshModel : MeshModelBase
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