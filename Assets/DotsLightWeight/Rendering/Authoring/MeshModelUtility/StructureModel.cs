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

namespace Abarabone.Structure.Authoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Model.Authoring;

    [Serializable]
    public class StructureModel<TIdx, TVtx> : LodMeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {

        public StructureModel(GameObject obj, Shader shader) : base(obj, shader)
        { }


        public override void CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType boneType = BoneType.TR;
            const int boneLength = 1;
            const int vectorOffsetPerInstance = 4;

            gcs.CreateDrawModelEntityComponents
                (this.Obj, mesh, mat, boneType, boneLength, vectorOffsetPerInstance);
        }


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
    }

}