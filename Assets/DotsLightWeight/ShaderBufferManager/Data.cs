using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

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
    }


    /// <summary>
    /// draw model 向け　カラーパレット関係のデータ
    /// </summary>
    public static class DrawModelShaderBuffer
    {
        /// <summary>
        /// カラーパレットのグラフィックバッファへのリンク
        /// </summary>
        public class ColorPaletteLinkData : IComponentData
        {
            public Entity BufferEntity;
        }
    }
}
