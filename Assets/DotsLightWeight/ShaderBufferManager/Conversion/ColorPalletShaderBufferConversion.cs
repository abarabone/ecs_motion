using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using System;
using System.Linq;
using System.Collections.Generic;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Colider = Unity.Physics.Collider;

namespace DotsLite.Draw.Authoring.Palette
{
    using DotsLite.Dependency;
    using DotsLite.Model.Authoring;
    using DotsLite.Structure;
    using DotsLite.EntityTrimmer.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Geometry.Palette;

    // モデル
    // ・palette id を持ったマテリアルを集計
    // 　→ 頂点の palett index を生成

    // インスタンス
    // ・使用するパレットをアセットで所持
    // 　→ ビルダーに登録して、最後にひとまとめに
    // 　　→ グラフィックバッファに出力
    // 　　→ インスタンスにはパレットデータを所持

    // パレット
    // ・モデルごとにパレットを持つ
    // ・そのパレットについての内部インデックスを、頂点ごとにもつ


    // カラーパレットはシーンで１つ、ということにする
    // プレハブからシーンのヒエラルキー親にアクセスできれば、複数のバッファにできるんだけど

    /// <summary>
    /// カラーパレットのソースデータを作成する。
    /// ソースデータは、グラフィックバッファを生成初期化するために使用する。
    /// </summary>
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class ColorPaletteShaderBufferConversion : GameObjectConversionSystem
    {


        public ColorPaletteBuilder Palettes { get; } = new ColorPaletteBuilder();

        public string BufferName = "ColorPalettes";



        protected override void OnUpdate()
        {
            var colors = this.Palettes.ToArray();
            if (colors.Length == 0) return;


            var em = this.DstEntityManager;

            //this.Entities
            //    .ForEach((Entity ent, ColorPaletteBufferAuthoring auth) =>
            //    {
            //        var buffer = auth.Palettes.BuildShaderBuffer();

            //        em.AddComponentData(ent, new ShaderBuffer.ColorPaletteData
            //        {
            //            Buffer = buffer,
            //            NameId = Shader.PropertyToID(auth.BufferName),
            //        });
            //    });

            var ent = em.CreateEntity();

            em.AddComponentData(ent, new ShaderBuffer.ColorPaletteSrcData
            {
                Colors = colors,
                NameId = Shader.PropertyToID(this.BufferName),
            });
        }

    }

    /// <summary>
    /// モデルインスタンスごとにカラーパレットを登録し、グラフィックバッファ用のカラー配列を構築する。
    /// またインスタンスには、バッファ内の位置をＩＤとして返す。
    /// </summary>
    public class ColorPaletteBuilder
    {

        Dictionary<string, (int i, Color32[] colors)> colors = new Dictionary<string, (int, Color32[])>();

        int nextIndex = 0;


        /// <summary>
        /// １モデルインスタンス分のパレットを登録し、ＩＤ（位置）を返す。
        /// </summary>
        public int RegistAndGetId(Color32[] values)
        {
            var key = toKey(values); Debug.Log(key);

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
