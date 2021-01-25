using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Abarabone.Geometry
{

    public class TextureAtlasDictionary : GameObjectConversionSystem
    {
        public class Data : IComponentData
        {
            // ゲームオブジェクト、プレハブからアトラスを取得する
            public Dictionary<GameObject, Texture2D> objectToAtlas;

            // アトラスとパートテクスチャからＵＶ調整矩形を取得する
            public HashToRect texHashToUvRect;
        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.GetSingleton<Data>().objectToAtlas.Clear();
            this.GetSingleton<Data>().texHashToUvRect.dict.Clear();
            this.EntityManager.DestroyEntity(this.GetSingletonEntity<Data>());
        }
    }

    public static class TextureAtlasDictionaryExtension
    {

        public static TextureAtlasDictionary.Data GetTextureAtlasHolder(this GameObjectConversionSystem gcs)
        {
            var holder = gcs.GetSingleton<TextureAtlasDictionary.Data>();
            if (holder != null) return holder;

            var newHolder = new TextureAtlasDictionary.Data
            {
                objectToAtlas = new Dictionary<GameObject, Texture2D>(),
                texHashToUvRect = new Dictionary<(int atlas, int part), Rect>(),
            };
            gcs.SetSingleton(holder);
            return newHolder;
        }

    }
}
