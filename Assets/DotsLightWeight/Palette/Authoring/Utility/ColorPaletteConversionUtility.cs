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
    public static class ColorPaletteConversionUtility
    {

        /// <summary>
        /// インスタンス１つに対し、パレットＩＤデータを追加する。
        /// ＩＤは、パレットのバッファ中の位置。パレットは PaletteAsset から取得する。
        /// </summary>
        public static void AddColorPaletteComponents(this GameObjectConversionSystem gcs, Entity ent, ColorPaletteAsset palette = null)
        {
            var paletteIdBase = palette == null
                ? gcs.GetColorPaletteBuilder().RegistAndGetId(palette.Colors)
                : 0;

            var em = gcs.DstEntityManager;
            em.AddComponentData(ent, new PaletteInstance.ColorPaletteData
            {
                BaseIndex = paletteIdBase,
            });
            em.AddComponentData(ent, new Draw.DrawInstance.TransferSpecialTag { });
        }
    }


}
