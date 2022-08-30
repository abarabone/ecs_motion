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

        Dictionary<int, Dictionary<string, int>> texIdDict = new 



        /// <summary>
        /// １モデルインスタンス分のパレットを登録し、ＩＤ（位置）を返す。
        /// </summary>
        public int RegistAndGetId(Texture2D atlas, Texture2D[] subtexs)
        {
            var key = tokey_(subtexs);

            if (this.texIdDict.TryGetValue(key, out var id))
            {
                return id;
            }

            var nextid = this.texIdDict.Count;

            this.texIdDict.Add(key, nextid);

            return nextid;


            static string toKey_(Texture2D[] keysrc)
            {
                var q =
                    from x in keysrc
                    select x.GetHashCode()
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
                from x in this.texIdDict.Keys
                from y in x.subtexs
                select hashToRect[atlasHash, y.GetHashCode()]
                ;
            return q.ToArray();
        }
    }

}
