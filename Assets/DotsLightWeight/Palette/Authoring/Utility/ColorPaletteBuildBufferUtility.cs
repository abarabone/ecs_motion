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
    /// インスタンス用カラーパレット登録ユーティリティ
    /// </summary>
    public static class ColorPaletteDataUtility
    {

        /// <summary>
        /// システム経由でカラーパレットビルダーを取得する。
        /// </summary>
        /// <returns></returns>
        public static ColorPaletteBufferBuilder GetColorPaletteBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetOrCreateSystem<ColorPaletteShaderBufferConversion>().Builder;
        }


        /// <summary>
        /// パレット配列から、グラフィックバッファーを構築する。
        /// </summary>
        public static GraphicsBuffer BuildColorPaletteShaderBuffer(this uint[] colors)
        {
            if (colors.Length == 0) return null;

            //var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(uint4));
            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, colors.Length, sizeof(uint));

            buf.SetData(colors);

            return buf;
        }





        ///// <summary>
        ///// モデル１つ分のパレット配列を生成する。
        ///// </summary>
        //public static Color32[] ToColorPaletteEntry(
        //    this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts)
        //{
        //    // ・モデルから sub index ごとの色を抽出
        //    // ・color palette に登録、最後にバッファを構築
        //    // ・バッファはシーンに１つ
        //    // ・color palette の base index を、インスタンスに持たせる
        //    // ・ただし、すでに同じ構成で登録があれば、その base index を取得する
        //    // １つのモデルを構成する幾何情報から、カラーパレットを構成するカラーを抽出する。
        //    // 結果はカラーの配列となる。（つまり、カラーパレット１つは、モデル１つに対して作成される）
        //    // カラーのインデックスはマテリアルの Palette Sub Index プロパティにユーザーがセットする。
        //    // 結果の配列は、そのインデックス順にソートされており、インデックスに該当するマテリアルが存在しなかった場合は、
        //    // (0, 0, 0, 0) 色がせっとされる。
        //    var q =
        //        from mmt in mmts
        //        from mat in mmt.mats
        //        select (index: getPaletteSubIndex_(mat), color: (Color32)mat.color)
        //        ;
        //    var colors = q.ToLookup(x => x.index, x => x.color);
        //    var maxIndex = colors.Max(x => x.Key);
        //    var qResult =
        //        from i in Enumerable.Range(0, maxIndex + 1)
        //        select colors.Contains(i)
        //            ? colors[i].First()
        //            : new Color32()
        //        ;
        //    return qResult.ToArray();


        //    /// <summary>
        //    /// マテリアルから、パレットインデックス情報を取得する。
        //    /// 該当するプロパティがない場合のインデックスは、0 とする。
        //    /// </summary>
        //    static int getPaletteSubIndex_(Material mat) =>
        //        //mat?.HasInt("Palette Sub Index") ?? false
        //        mat?.HasProperty("Palette Sub Index") ?? false
        //            ? mat.GetInt("Palette Sub Index")
        //            : 0
        //        ;
        //}

    }



    /// <summary>
    /// モデルインスタンスごとにカラーパレットを登録し、グラフィックバッファ用のカラー配列を構築する。
    /// またインスタンスには、バッファ内の位置をＩＤとして返す。
    /// </summary>
    public class ColorPaletteBufferBuilder
    {

        Dictionary<string, (int id, Color32[] colors)> colors = new Dictionary<string, (int, Color32[])>();

        int nextIndex = 0;


        /// <summary>
        /// １モデルインスタンス分のパレットを登録し、ＩＤ（位置）を返す。
        /// </summary>
        public int RegistAndGetId(Color32[] values)
        {
            var key = toKey(values); Debug.Log(key);

            if (this.colors.TryGetValue(key, out var x))
            {
                return x.id;
            }

            this.colors[key] = (this.nextIndex, values);

            return addIndex_(values.Length);


            static string toKey(Color32[] keysrc)
            {
                var q =
                    from x in keysrc
                    select $"{x.r},{x.g},{x.b},{x.a}"
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
        /// 登録されたすべてのカラー配列を返す。
        /// </summary>
        public uint[] ToArray()
        {
            var q =
                from x in this.colors
                from y in x.Value.colors
                select y.ToUint()//y
                ;
            return q.ToArray();
        }
    }

}
