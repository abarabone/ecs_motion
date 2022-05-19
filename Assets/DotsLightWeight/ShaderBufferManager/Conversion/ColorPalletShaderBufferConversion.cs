using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Colider = Unity.Physics.Collider;

namespace DotsLite.Draw.Authoring
{
    using DotsLite.Dependency;
    using DotsLite.Model.Authoring;
    using DotsLite.Structure;
    using DotsLite.EntityTrimmer.Authoring;
    using DotsLite.Geometry;

    // カラーパレットはシーンで１つ、ということにする
    // プレハブからシーンのヒエラルキー親にアクセスできれば、複数のバッファにできるんだけど
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class ColorPaletteShaderBufferConversion : GameObjectConversionSystem
    {


        public ColorPaletteBuilder Palettes { get; } = new ColorPaletteBuilder();

        public string BufferName = "ColorPalettes";



        protected override void OnUpdate()
        {
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
            var colors = this.Palettes.ToArray();
            if (colors.Length == 0) return;

            em.AddComponentData(ent, new ShaderBuffer.ColorPaletteSrcData
            {
                Colors = colors,
                NameId = Shader.PropertyToID(this.BufferName),
            });
        }

    }
}
