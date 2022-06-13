using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Model.Authoring.Vertex.CharacterModel
{
    using DotsLite.Geometry;

    [Serializable]
    public class PositionNormalUvBonedVertex : PositionNormalUvBonedVertexBuilder, Authoring.CharacterModel.IVertexSelector
    { }
}

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw.Authoring;
    using DotsLite.Character;
    using DotsLite.Common.Extension;
    using DotsLite.CharacterMotion.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Draw;

    [Serializable]
    public class CharacterModel : MeshModelBase
    {


        public interface IVertexSelector : IVertexBuilder { }

        [SerializeReference, SubclassSelector]
        public IVertexSelector vtxBuilder;

        protected override IVertexBuilder VtxBuilder => this.vtxBuilder;




        public Transform boneTop =>
            this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().bones.First();


        public override Transform TfRoot => this.gameObject.Children().First().transform;// これでいいのか？

        public override IEnumerable<Transform> QueryBones => this.boneTop.gameObject.DescendantsAndSelf()
            .Where(x => !x.name.StartsWith("_"))
            //.Do(x => Debug.Log(x.name))
            .Select(x => x.transform);


        protected override int optionalVectorLength => 0;
        protected override int boneLength => this.QueryBones.Count();

    }

}

