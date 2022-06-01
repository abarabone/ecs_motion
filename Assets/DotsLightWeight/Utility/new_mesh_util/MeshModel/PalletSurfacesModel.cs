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

    // ・

    //[Serializable]
    //public class PaletteSurfacesModel : MeshModel<UI16, StructureVertex>
    //{


    //    public override (SourcePrefabKeyUnit key, Func<IMeshElements> f) BuildMeshCombiner(
    //        SrcMeshesModelCombinePack meshpack,
    //        Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary)
    //    {

    //        return (this.sourcePrefabKey, () =>
    //        {

    //            return createMesh_();
    //        });



    //        MeshElements<UI16, StructureVertex> createMesh_()
    //        {
    //            return new MeshElements<UI16, StructureVertex>
    //            {

    //            };
    //        }
    //    }

    //}

}