using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;

    /// <summary>
    /// テクスチャアトラスに関する情報を登録するための辞書。
    /// 扱う辞書は、モデル→アトラス辞書、元テクスチャ→ＵＶオフセット辞書。
    /// </summary>
    public class TextureAtlasDictionary : GameObjectConversionSystem
    {
        public class Data : IComponentData
        {
            // モデルプレハブからアトラスを取得する
            // モデルとアトラスは１対１ではなく、多対１
            public Dictionary<SourcePrefabKeyUnit, Texture2D> modelToAtlas;

            // アトラスとパートテクスチャからＵＶ調整矩形を取得する
            public HashToRect texHashToUvRect;
        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (this.HasSingleton<Data>())
            {
                this.EntityManager.DestroyEntity(this.GetSingletonEntity<Data>());
            }
        }
    }

    public static class TextureAtlasDictionaryExtension
    {

        public static TextureAtlasDictionary.Data GetTextureAtlasDictionary(this GameObjectConversionSystem gcs)
        {
            if (!gcs.HasSingleton<TextureAtlasDictionary.Data>()) return create_();

            return gcs.GetSingleton<TextureAtlasDictionary.Data>();


            TextureAtlasDictionary.Data create_()
            {
                var newent = gcs.EntityManager.CreateEntity(typeof(TextureAtlasDictionary.Data));
                var newholder = new TextureAtlasDictionary.Data
                {
                    modelToAtlas = new Dictionary<SourcePrefabKeyUnit, Texture2D>(),
                    texHashToUvRect = new HashToRect(),
                };
                gcs.EntityManager.SetComponentData(newent, newholder);
                return newholder;
            }
        }



        //public static void SetAtlasToDictionary
        //    (this GameObjectConversionSystem gcs, GameObject obj, TextureAtlasAndParameter atlasParams)
        //{
        //    var dict = gcs.GetTextureAtlasDictionary();

        //    dict.objectToAtlas[obj] = atlasParams.atlas;

        //    foreach (var (hash, uv) in (atlasParams.texhashes, atlasParams.uvRects).Zip())
        //    {
        //        dict.texHashToUvRect[hash.atlas, hash.part] = uv;
        //    }
        //}

    }
}
