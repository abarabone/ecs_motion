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
        public static void SetColorPalette_(GameObjectConversionSystem gcs, Entity ent, ColorPaletteAsset palette)
        {
            if (palette == null) return;

            var em = gcs.DstEntityManager;
            var paletteIdBase = gcs.GetColorPaletteBuilder().RegistAndGetId(palette.Colors);
            em.AddComponentData(ent, new Draw.DrawInstance.TransSpecialferTag { });
            em.AddComponentData(ent, new Palette.ColorPaletteData
            {
                BaseIndex = paletteIdBase,
            });
        }
    }
}
