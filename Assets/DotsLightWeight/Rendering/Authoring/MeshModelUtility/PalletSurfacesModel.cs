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
    using DotsLite.Structure.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Model.Authoring;

    [Serializable]
    public class PalletSurfacesModel : MeshModel<UI16, StructureVertex>
    {

        public override Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType BoneType = BoneType.RT;
            const int boneLength = 1;
            const int instanceDataVectorLength = 1;

            return gcs.CreateDrawModelEntityComponents(
                mesh, mat, BoneType, boneLength, DrawModel.SortOrder.desc, instanceDataVectorLength);
        }


        public override (SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary)
        {
            //var atlas = atlasDictionary.srckeyToAtlas[this.sourcePrefabKey].GetHashCode();
            //var texdict = atlasDictionary.texHashToUvRect;
            //return (
            //    this.sourcePrefabKey,
            //    meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, this.QueryBones?.ToArray(),
            //        part => texdict[atlas, part],
            //        isubmesh => 0)
            //);

            return (this.sourcePrefabKey, () =>
            {

                return createMesh_();
            });

            //Dictionary<int, int> buildTexHashToUvIndexDictionary_()
            //{

            //}

            MeshElements<UI16, StructureVertex> createMesh_()
            {
                return new MeshElements<UI16, StructureVertex>
                {

                };
            }
        }

    }

}