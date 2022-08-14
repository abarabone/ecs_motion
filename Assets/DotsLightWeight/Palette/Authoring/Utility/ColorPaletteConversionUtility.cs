using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Geometry.Palette
{
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;

    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utility;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring.Palette;

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

    /// <summary>
    /// インスタンス用カラーパレット登録ユーティリティ
    /// </summary>
    public static class ColorPaletteDataUtility
    {

        /// <summary>
        /// インスタンス１つに対し、パレットＩＤデータを追加する。
        /// ＩＤは、パレットのバッファ中の位置。パレットはアセットから取得する。
        /// </summary>
        public static void SetColorPaletteComponent(this GameObjectConversionSystem gcs, GameObject main, ColorPaletteAsset palette)
        {
            //if (model.GetType().GetGenericTypeDefinition() != typeof(MeshWithPaletteModel<,>).GetGenericTypeDefinition()) return;
            if (palette == null) return;

            var em = gcs.DstEntityManager;
            var ent = gcs.GetPrimaryEntity(main);

            em.AddComponentData(ent, new Palette.ColorPaletteData
            {
                BaseIndex = gcs.GetColorPaletteBuilder().RegistAndGetId(palette.Colors),
            });
        }

        /// <summary>
        /// システム経由でカラーパレットビルダーを取得する。
        /// </summary>
        /// <returns></returns>
        public static ColorPaletteBuilder GetColorPaletteBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetExistingSystem<ColorPaletteShaderBufferConversion>().Palettes;
        }

        /// <summary>
        /// パレット配列から、グラフィックバッファーを構築する。
        /// </summary>
        public static GraphicsBuffer BuildShaderBuffer(this uint[] colors)
        {
            if (colors.Length == 0) return null;

            //var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(uint4));
            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, colors.Length, sizeof(uint));

            buf.SetData(colors);

            return buf;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public static void AddPalletLinkData_IfHas(this IMeshModel model, GameObjectConversionSystem gcs, Entity ent)
        //{
        //    var component = model as MonoBehaviour;
        //    var paletteAuthor = component.GetComponentInParent<ColorPaletteBufferAuthoring>();
        //    if (paletteAuthor == null) return;

        //    var em = gcs.DstEntityManager;
        //    em.AddComponentData(ent, new DrawModelShaderBuffer.ColorPaletteLinkData
        //    {
        //        BufferEntity = gcs.GetPrimaryEntity(paletteAuthor),
        //    });
        //}
    }
}
