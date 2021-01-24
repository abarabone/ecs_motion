using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Abarabone.Geometry
{

    public class TextureAtlasHolderData : IComponentData
    {
        public Dictionary<TextureHashKey, Rect> texHashToUvRect;
    }

    public static class TextureAtlasHolder
    {

        public static TextureAtlasHolderData GetTextureAtlasHolder(this GameObjectConversionSystem gcs)
        {
            var holder = gcs.GetSingleton<TextureAtlasHolderData>();
            if (holder != null) return holder;

            var newHolder = new TextureAtlasHolderData
            {
                texHashToUvRect = new Dictionary<int, Rect>(),
            };
            gcs.SetSingleton(holder);
            return newHolder;
        }

    }
}
