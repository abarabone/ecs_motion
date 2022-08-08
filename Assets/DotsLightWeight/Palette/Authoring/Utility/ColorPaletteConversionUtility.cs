using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Geometry.Palette
{
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;

    public static class ColorPaletteConversionUtility
    {

        public static void SetColorPalette(this GameObjectConversionSystem gcs, Entity ent, ColorPaletteAsset palette)
        {
            if (palette == null) return;

            var em = gcs.DstEntityManager;
            var paletteIdBase = gcs.GetColorPaletteBuilder().RegistAndGetId(palette.Colors);
            em.AddComponentData(ent, new Palette.ColorPaletteData
            {
                BaseIndex = paletteIdBase,
            });
            em.AddComponentData(ent, new Draw.DrawInstance.TransferSpecialTag { });
        }


        public static void AddColorPalletComponents(this GameObjectConversionSystem gcs, Entity ent)
        {
            var em = gcs.DstEntityManager;
            em.AddComponentData(ent, new Palette.ColorPaletteData { });
            em.AddComponentData(ent, new Draw.DrawInstance.TransferSpecialTag { });
        }
    }
}
