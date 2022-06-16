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

namespace DotsLite.Structure.Authoring.Vertex.AreaModel
{
    using DotsLite.Geometry;

    [Serializable]
    public class PositionNormalUv :
        StructureVertexBuilder, Authoring.AreaModel.IVertexSelector
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
    public class AreaModel : BuildingModel//LodMeshModelBase
    {

        protected override int optionalVectorLength => 4;
        protected override int boneLength => 1;


        //public AreaModel(GameObject obj, Shader shader) : base(obj, shader)
        //{ }


        //public override Transform TfRoot => this.Obj.GetComponentsInParent<StructureBuildingModelAuthoring>(true)
        //    .FirstOrDefault()
        //    .transform;

        //public override Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        //{
        //    var mat = new Material(this.shader);
        //    mat.enableInstancing = true;
        //    mat.mainTexture = atlas;

        //    var boneType = this.tfMode.ToBoneType();
        //    const int boneLength = 1;
        //    const int vectorOffsetPerInstance = 4;

        //    return gcs.CreateDrawModelEntityComponents(
        //        mesh, mat, boneType, boneLength, DrawModel.SortOrder.desc, vectorOffsetPerInstance);
        //}


        //public override (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner
        //    (
        //        SrcMeshesModelCombinePack meshpack,
        //        Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
        //    )
        //{
        //    var atlas = atlasDictionary.objectToAtlas[this.Obj].GetHashCode();
        //    var texdict = atlasDictionary.texHashToUvRect;
        //    return (
        //        this.Obj,
        //        meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, part => texdict[atlas, part], this.Bones)
        //    );
        //}



        //public interface IVertexSelector : IVertexBuilder { }


        //[SerializeReference, SubclassSelector]
        //public IIndexSelector idxBuilder;
        //protected override IIndexBuilder IdxBuilder => this.idxBuilder;

        //[SerializeReference, SubclassSelector]
        //public IVertexSelector vtxBuilder;
        //protected override IVertexBuilder VtxBuilder => this.vtxBuilder;
    }

}