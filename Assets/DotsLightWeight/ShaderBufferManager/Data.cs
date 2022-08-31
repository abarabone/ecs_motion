using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Draw
{

    /// <summary>
    /// カラーパレットのグラフィックバッファデータ
    /// </summary>
    public static class ShaderBuffer
    {
        /// <summary>
        /// コンバージョン時に作成するソースデータ
        /// </summary>
        public class ColorPaletteSrcData : IComponentData
        {
            public uint[] Colors;
            public int NameId;
        }
        /// <summary>
        /// グラフィックバッファ
        /// </summary>
        public class ColorPaletteData : IComponentData
        {
            public GraphicsBuffer Buffer;
            public int NameId;
        }


        /// <summary>
        /// コンバージョン時に作成するソースデータ
        /// </summary>
        public class UvPaletteSrcData : IComponentData
        {
            public float4[] UvOffsets;
            public int NameId;
        }
        /// <summary>
        /// グラフィックバッファ
        /// </summary>
        public class UvPaletteData : IComponentData
        {
            public GraphicsBuffer Buffer;
            public int NameId;
        }
    }


    /// <summary>
    /// draw model 向け　カラーパレット関係のデータ
    /// </summary>
    public static class DrawModelWithPalette
    {
        /// <summary>
        /// カラーパレットのグラフィックバッファへのリンク
        /// </summary>
        public class ColorPaletteLinkData : IComponentData
        {
            public Entity ShaderBufferEntity;
        }

        /// <summary>
        /// ＵＶパレットのグラフィックバッファへのリンク
        /// </summary>
        public class UvPaletteLinkData : IComponentData
        {
            public Entity ShaderBufferEntity;
        }
    }
}
