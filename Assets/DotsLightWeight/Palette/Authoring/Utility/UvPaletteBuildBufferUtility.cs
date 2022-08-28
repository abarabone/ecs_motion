using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

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
    /// インスタンス用カラーパレット登録ユーティリティ
    /// </summary>
    public static class UvPaletteDataUtility
    {

        /// <summary>
        /// システム経由でカラーパレットビルダーを取得する。
        /// </summary>
        /// <returns></returns>
        //public static UvPaletteBuilder GetUvPaletteBuilder(this GameObjectConversionSystem gcs)
        //{
        //    return gcs.World.GetExistingSystem<ColorPaletteShaderBufferConversion>().Builder;
        //}


        /// <summary>
        /// パレット配列から、グラフィックバッファーを構築する。
        /// </summary>
        public static GraphicsBuffer BuildUvPaletteShaderBuffer(this Rect[] uvrects)
        {
            if (uvrects.Length == 0) return null;

            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, uvrects.Length, sizeof(float) * 4);

            buf.SetData(uvrects);

            return buf;
        }

    }



    /// <summary>
    /// モデルインスタンスごとにＵＶパレットを登録し、グラフィックバッファ用のＵＶ配列を構築する。
    /// またインスタンスには、バッファ内の位置をＩＤとして返す。
    /// </summary>
    public class UvPaletteBufferBuilder
    {

        Dictionary<string, (int id, Texture2D atlas, Texture2D[] subtexs)> texs
            = new Dictionary<string, (int id, Texture2D atlas, Texture2D[] subtexs)>();

        //int nextIndex = 0;

        HashSet<(Texture2D atlas, Texture2D[] subtexs)> texs = new();


        /// <summary>
        /// １モデルインスタンス分のパレットを登録し、ＩＤ（位置）を返す。
        /// </summary>
        public int RegistAndGetId(Texture2D atlas, Texture2D[] textures)
        {


            if (this.texs.(key, out var x))
            {
                return x.id;
            }

            //this.texs[key] = (this.nextIndex, values);

            return addIndex_(textures.Length);


            static string toKey_(, Texture2D[] keysrc)
            {
                var q =
                    from x in keysrc
                    select x.GetHashCode().ToString()
                    ;
                return string.Join("/", q);
            }

            int addIndex_(int length)
            {
                var index = this.nextIndex;
                this.nextIndex += length;
                return index;
            }
        }

        /// <summary>
        /// 登録されたすべてのＵＶオフセット配列を返す。
        /// </summary>
        public Rect[] ToUvRectArray(Texture2D atlas, HashToRect hashToRect)
        {
            var atlasHash = atlas.GetHashCode();
            var q =
                from x in this.texs
                from y in x.Value.subtexs
                select hashToRect[atlasHash, y.GetHashCode()]
                ;
            return q.ToArray();
        }
    }

}
