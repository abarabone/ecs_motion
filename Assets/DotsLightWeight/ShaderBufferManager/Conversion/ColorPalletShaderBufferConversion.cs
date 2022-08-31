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

    // パレット
    // ・パレットバッファは、カラー配列によってなる。
    // ・カラー配列は、パレット単位に区切られている。
    // ・パレットの base index は、パレットが始まる位置のカラーインデックス。

    // インスタンス
    // ・コンバートは、インスタンスごとのパレットを登録しつつ、base index を返してもらう。
    // ・使用するパレットをアセットで所持
    // 　→ ビルダーに登録して、最後にひとまとめに
    // 　→ グラフィックバッファに出力
    // 　→ インスタンスにはパレットデータを所持

    // モデル
    // ・パレットに対応したメッシュは、パレットについての内部インデックスを、頂点ごとにもつ。
    // ・コンバートは、palette id を持ったマテリアルを集計し、内部インデックスとする。
    // 　→ マテリアルに対応したサブメッシュごとに、頂点の palette index がセットされる。


    // カラーパレットはシーンで１つ、ということにする
    // プレハブからシーンのヒエラルキー親にアクセスできれば、複数のバッファにできるんだけど

    /// <summary>
    /// カラーパレットのソースデータを作成する。
    /// ソースデータは、グラフィックバッファを生成初期化するために使用する。
    /// </summary>
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class ColorPaletteShaderBufferConversion : GameObjectConversionSystem
    {


        public ColorPaletteBufferBuilder Builder { get; } = new ColorPaletteBufferBuilder();

        public string BufferName = "ColorPalettes";



        protected override void OnUpdate()
        {
            var colors = this.Builder.ToArray();
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

}
