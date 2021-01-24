using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Abarabone.Geometry
{

    public class TextureAtlasHolderData : IComponentData
    {
        // ゲームオブジェクト、プレハブからアトラスを取得する
        public Dictionary<GameObject, Texture2D> objectToAtlas;

        // アトラスとパートテクスチャからＵＶ調整矩形を取得する
        public HashToRect texHashToUvRect;
    }

    public static class TextureAtlasHolder
    {

        public static TextureAtlasHolderData GetTextureAtlasHolder(this GameObjectConversionSystem gcs)
        {
            var holder = gcs.GetSingleton<TextureAtlasHolderData>();
            if (holder != null) return holder;

            var newHolder = new TextureAtlasHolderData
            {
                objectToAtlas = new Dictionary<GameObject, Texture2D>(),
                texHashToUvRect = new Dictionary<(int atlas, int part), Rect>(),
            };
            gcs.SetSingleton(holder);
            return newHolder;
        }

    }
}
