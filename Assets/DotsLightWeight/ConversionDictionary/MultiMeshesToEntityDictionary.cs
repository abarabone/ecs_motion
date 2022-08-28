using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using System;

using DotsLite.Model;
using DotsLite.Draw.Authoring;
using DotsLite.Common.Extension;
using DotsLite.Draw;
using DotsLite.Model.Authoring;
using DotsLite.Character;//ObjectMain ÇÕÇ±Ç±Ç…Ç†ÇÈÅAñºëOïœÇ¶ÇÈÇ◊Ç´Ç©ÅH
using DotsLite.Structure;
using Unity.Physics;
using Unity.Transforms;
using DotsLite.Geometry;
using DotsLite.Utilities;
using DotsLite.EntityTrimmer.Authoring;

using Material = UnityEngine.Material;
using Unity.Physics.Authoring;

public class MultiMeshesToDrawModelEntityDictionary
{
    Dictionary<int, Mesh> dict;

    //public Mesh GetOrCreateMesh(IMeshModel srcModel)
    //{
    //    var mmts = srcModel.QueryMmts.ToArray();
    //    var hash = getMeshesHash_(mmts.Select(x => x.mesh));

    //    if (this.dict.ContainsKey(hash)) return this.dict[hash];

    //    var mesh = srcModel.BuildMeshCombiner()

    //    return;

    //    int getMeshesHash_(IEnumerable<Mesh> meshes)
    //    {
    //        var qHash =
    //            from mesh in meshes
    //            select mesh.GetHashCode()
    //            ;
    //        return meshes
    //            .Select(mesh => mesh.GetHashCode())
    //            .Select((x, i) => x * 31 ^ i)
    //            .Do(x => Debug.Log($"model {srcModel.Obj.name}, hash {x}"))
    //            .Sum();
    //    }
    //}
}
