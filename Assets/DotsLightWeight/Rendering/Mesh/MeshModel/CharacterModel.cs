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

        Transform boneTop =>
            this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().bones.First();


        //public override Transform TfRoot => this.transform;//this.gameObject.Children().First().transform;// これでいいのか？

        IEnumerable<Transform> queryBones => this.boneTop.gameObject.DescendantsAndSelf()
            .Where(x => !x.name.StartsWith("_"))
            //.Do(x => Debug.Log($"dst bone : {x.name}"))
            .Select(x => x.transform);


        protected override int optionalVectorLength => 0;
        protected override int boneLength => this.queryBones.Count();


        public override Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var p = new AdditionalParameters();
            var atlas = atlasDictionary.modelToAtlas[this.sourcePrefabKey].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var mmts = this.QueryMmts.ToArray();
            p.calculateParameters(mmts, this.Obj.transform, subtexhash => texdict[atlas, subtexhash]);
            p.calculateBoneParameters(mmts, this.queryBones.ToArray());

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

