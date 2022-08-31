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
        public static UvPaletteBufferBuilder GetUvPaletteBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetOrCreateSystem<UvPaletteShaderBufferConversion>().Builder;
        }


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


        /// <summary>
        /// アトラスごとに作成される、ＵＶパレットバッファを格納するエンティティを取得する。
        /// </summary>
        public static Entity GetUvPaletteEntity(this GameObjectConversionSystem gcs, Texture2D atlas)
        {
            var holder = gcs.World.GetOrCreateSystem<UvPaletteShaderBufferConversion>().EntityHolder;

            if (holder.TryGetValue(atlas, out var ent))
            {
                return ent;
            }

            return holder[atlas] = ent;
        }

    }



    /// <summary>
    /// モデルインスタンスごとにＵＶパレットを登録し、グラフィックバッファ用のＵＶ配列を構築する。
    /// またインスタンスには、バッファ内の位置をＩＤとして返す。
    /// </summary>
    public class UvPaletteBufferBuilder
    {

        Dictionary<int, Dictionary<string, int>> dictHolder =
            new Dictionary<int, Dictionary<string, int>>();



        /// <summary>
        /// １モデルインスタンス分のパレットを登録し、ＩＤ（位置）を返す。
        /// </summary>
        public int RegistAndGetId(Texture2D atlas, Texture2D[] subtexs)
        {
            var dict = getInnerDict_(atlas.GetHashCode());
            var key = toKey_(subtexs);

            if (dict.TryGetValue(key, out var id))
            {
                return id;
            }

            return dict[key] = dict.Count;


            Dictionary<string, int> getInnerDict_(int atlasHash)
            {
                if (this.dictHolder.TryGetValue(atlasHash, out var innerDict))
                {
                    return innerDict;
                }

                return this.dictHolder[atlasHash] = new Dictionary<string, int>();
            }

            static string toKey_(Texture2D[] keysrc)
            {
                var q =
                    from x in keysrc
                    select x.GetHashCode()
                    ;
                return string.Join("/", q);
            }
        }

        /// <summary>
        /// 登録されたＵＶオフセット配列を返す。
        /// ＵＶやアトラスが登録されていない場合でも、空の配列を返す。
        /// </summary>
        public Rect[] ToUvRectArray(Texture2D atlas, HashToRect hashToRect)
        {
            var atlasHash = atlas.GetHashCode();
            if (this.dictHolder.TryGetValue(atlasHash, out var innerDict))
            {
                return new Rect[0];
            }

            var q =
                from x in innerDict.Keys
                from y in x.Split('/')
                let subtexHash = int.Parse(y)
                select hashToRect[atlasHash, subtexHash]
                ;
            return q.ToArray();
        }
    }

}
