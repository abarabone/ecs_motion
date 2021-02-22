using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Authoring
{
    using Abarabone.Geometry;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Draw.Authoring;

    static public class ModelTexturePackingUtility
    {

        static public void PackTextureToDictionary
            (this IEnumerable<GameObject> objs, TextureAtlasDictionary.Data atlasDict)
        {
            var texobjs = objs
                .Where(x => !atlasDict.objectToAtlas.ContainsKey(x))
                //.Logging(x => x.name)
                .ToArray();

            if (texobjs.Length == 0) return;

            var qMat =
                from obj in texobjs
                from r in obj.GetComponentsInChildren<Renderer>()
                from mat in r.sharedMaterials
                select mat
                ;

            var tex = qMat.QueryUniqueTextures().ToAtlasOrPassThroughAndParameters();

            atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
            atlasDict.objectToAtlas.AddRange(texobjs, tex.atlas);
        }
    }
}
