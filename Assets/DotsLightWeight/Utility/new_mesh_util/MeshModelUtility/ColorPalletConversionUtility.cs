using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utility;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;

    public class ColorPalletBuilder
    {

        Dictionary<string, (int i, Color32[] colors)> colors = new Dictionary<string, (int, Color32[])>();

        int nextIndex = 0;


        public int RegistAndGetId(Color32[] values)
        {
            var key = toKey(values);Debug.Log(key);

            if (this.colors.TryGetValue(key, out var x))
            {
                return x.i;
            }

            var index = this.nextIndex;
            this.colors[key] = (index, values);
            this.nextIndex += values.Length;
            return index;


            static string toKey(Color32[] keysrc)
            {
                var q =
                    from x in keysrc
                    select $"{x.r},{x.g},{x.b},{x.a}"
                    ;
                return string.Join("/", q);
            }
        }


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

    //public class ColorPalletBuilder
    //{

    //    List<Color32> colors = new List<Color32>();


    //    public int AddAndGetId(Color32[] values)
    //    {
    //        var id = this.colors.Count;

    //        this.colors.AddRange(values);

    //        return id;
    //    }

    //    public unsafe GraphicsBuffer BuildShaderBuffer()
    //    {
    //        var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(Color32));

    //        buf.SetData(this.colors);

    //        return buf;
    //    }
    //}


    public static partial class ColorPalletConversionUtility
    {

        /// <summary>
        /// パレットのサブインデックスを、サブメッシュ単位で列挙する。
        /// サブインデックスは、マテリアルの pallet sub index から取得する。
        /// マテリアルが null の場合は、0 を返す。
        /// </summary>
        public static void CalculatePalletSubIndexParameter(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
            ref AdditionalParameters p)
        {
            var q =
                from mmt in mmts
                select
                    from mat in mmt.mats
                    select mat.GetPalletSubIndex()
                ;
            p.palletSubIndexPerSubMesh = q.ToArrayRecursive2();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public static void CalculateUvIndexParameter(
        //    this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
        //    ref AdditionalParameters p,
        //    IEnumerable<int> texhashesForUvSurface)
        //{
        //    var q =
        //        from mmt in mmts
        //        select
        //            from mat in mmt.mats
        //            select mat?.mainTexture?.GetHashCode() ?? 0
        //        ;
        //    var texhashPerSubMesh = q.ToArrayRecursive2();

        //    var usedTexhashes = texhashPerSubMesh
        //        .SelectMany(x => x)
        //        .Distinct()
        //        .ToArray();

        //    p.texhashPerSubMesh = texhashPerSubMesh;

        //}
        // tex rect は atlas 単位
        // atlas 単位で buffer 化
        // buffer には uv surface しか登録しない
        // uv surfaces の sub mesh ごとに sub index
        // 頂点に uv sub index を持たせる
        // 同じ uv sub index は同じ surface 
        // uv は同じ面構成単位で base index 指定　色違いでも同じ物体ならＯＫ
        // エディタでは、サブメッシュ単位でテクスチャを指定する scriptable object を持たせる
        // sub index は pallet と同じマテリアルの pallet sub index でよい
        // pallet はカラーセット単位で base index 指定


        public static int GetPalletSubIndex(this Material mat) =>
            //mat?.HasInt("Pallet Sub Index") ?? false
            mat?.HasProperty("Pallet Sub Index") ?? false
                ? mat.GetInt("Pallet Sub Index")
                : 0
            ;


        // ・モデルから sub index ごとの色を抽出
        // ・color pallet に登録、最後にバッファを構築
        // ・バッファはシーンに１つ
        // ・color pallet の base index を、インスタンスに持たせる
        // ・ただし、すでに同じ構成で登録があれば、その base index を取得する
        /// <summary>
        /// １つのモデルを構成する幾何情報から、カラーパレットを構成するカラーを抽出する。
        /// 結果はカラーの配列となる。（つまり、カラーパレット１つは、モデル１つに対して作成される）
        /// カラーのインデックスはマテリアルの Pallet Sub Index プロパティにユーザーがセットする。
        /// 結果の配列は、そのインデックス順にソートされており、インデックスに該当するマテリアルが存在しなかった場合は、
        /// (0, 0, 0, 0) 色がせっとされる。
        /// </summary>
        public static Color32[] ToPalletColorEntry(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts)
        {
            var q =
                from mmt in mmts
                from mat in mmt.mats
                select (index: mat.GetPalletSubIndex(), color: (Color32)mat.color)
                ;
            var colors = q.ToLookup(x => x.index, x => x.color);
            var maxIndex = colors.Max(x => x.Key);
            var qResult =
                from i in Enumerable.Range(0, maxIndex + 1)
                select colors.Contains(i)
                    ? colors[i].First()
                    : new Color32()
                ;
            return qResult.ToArray();
        }


        public static GraphicsBuffer BuildShaderBuffer(this uint[] colors)
        {
            if (colors.Length == 0) return null;

            //var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(uint4));
            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, colors.Length, sizeof(uint));

            buf.SetData(colors);

            return buf;
        }


        public static void SetColorPalletComponent(this GameObjectConversionSystem gcs, GameObject main, ColorPalletAsset pallet)
        {
            //if (model.GetType().GetGenericTypeDefinition() != typeof(MeshWithPalletModel<,>).GetGenericTypeDefinition()) return;
            if (pallet == null) return;

            var em = gcs.DstEntityManager;
            var ent = gcs.GetPrimaryEntity(main);

            em.AddComponentData(ent, new Pallet.PalletData
            {
                BaseIndex = gcs.GetColorPalletBuilder().RegistAndGetId(pallet.Colors),
            });
        }

        public static ColorPalletBuilder GetColorPalletBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetExistingSystem<ColorPalletShaderBufferConversion>().Pallets;
        }
    }
}
