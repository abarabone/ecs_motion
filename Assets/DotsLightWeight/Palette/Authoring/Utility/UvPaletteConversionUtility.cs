using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Geometry.Palette
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utility;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring.Palette;

    /// <summary>
    /// エンティティ生成
    /// </summary>
    public static class UvPaletteConversionUtility
    {

        /// <summary>
        /// インスタンス１つに対し、パレットＩＤデータを追加する。
        /// ＩＤは、パレットのバッファ中の位置。パレットは PaletteAsset から取得する。
        /// パレットバッファビルダーは、モデル単位で作成される。
        /// </summary>
        public static void AddUvPaletteComponents(
            this GameObjectConversionSystem gcs, Entity ent, UvPaletteBufferBuilder builder, UvPaletteAsset palette = null)
        {
            var paletteIdBase = palette == null
                ? builder.RegistAndGetId(palette.SubTextures)
                : 0;

            var em = gcs.DstEntityManager;
            em.AddComponentData(ent, new Palette.UvPaletteData
            {
                BaseIndex = paletteIdBase,
            });
            em.AddComponentData(ent, new Draw.DrawInstance.TransferSpecialTag { });
        }
    }


}
