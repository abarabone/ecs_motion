using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Draw.Authoring;

    static public class ModelTexturePackingUtility
    {

        /// <summary>
        /// モデル集合内のテクスチャから、１つのアトラスを生成する。
        /// アトラスは辞書に登録される。
        /// ただし、モデルに対してアトラスが登録済みなら、生成しない。
        /// </summary>
        static public void PackTextureToDictionary(
            this IEnumerable<IMeshModel> models, TextureAtlasDictionary.Data atlasDict)
        {
            var texmodels = models
                .Where(x => !atlasDict.modelToAtlas.ContainsKey(x.SourcePrefabKey))
                //.Logging(x => x.name)
                .ToArray();

            if (texmodels.Length == 0) return;

            var qMat =
                from model in texmodels
                from r in model.Obj.GetComponentsInChildren<Renderer>()
                from mat in r.sharedMaterials
                select mat
                ;

            var tex = qMat.QueryUniqueTextures().ToAtlasOrPassThroughAndParameters();

            atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
            atlasDict.modelToAtlas.AddEach(texmodels.Do(x => Debug.Log(x.Obj.name)).Select(x => x.SourcePrefabKey), tex.atlas);
        }
    }
}
