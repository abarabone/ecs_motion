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
    public class CharacterModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {

        [HideInInspector]
        public Transform boneTop;


        public override Transform TfRoot => this.objectTop.Children().First().transform;// これでいいのか？

        public override IEnumerable<Transform> QueryBones => this.boneTop.gameObject.DescendantsAndSelf()
            .Where(x => !x.name.StartsWith("_"))
            .Select(x => x.transform);


        protected override int optionalVectorLength => 0;
        protected override int boneLength => this.QueryBones.Count();

    }

}

