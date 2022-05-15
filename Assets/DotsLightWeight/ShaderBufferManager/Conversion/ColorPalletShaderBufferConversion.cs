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
    public class ColorPalletShaderBufferConversion : GameObjectConversionSystem
    {


        public ColorPalletBuilder Pallets { get; } = new ColorPalletBuilder();

        public string BufferName = "ColorPallets";



        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            //this.Entities
            //    .ForEach((Entity ent, ColorPalletBufferAuthoring auth) =>
            //    {
            //        var buffer = auth.Pallets.BuildShaderBuffer();

            //        em.AddComponentData(ent, new ShaderBuffer.ColorPalletData
            //        {
            //            Buffer = buffer,
            //            NameId = Shader.PropertyToID(auth.BufferName),
            //        });
            //    });

            var ent = em.CreateEntity();
            var colors = this.Pallets.ToArray();
            if (colors.Length == 0) return;

            em.AddComponentData(ent, new ShaderBuffer.ColorPalletSrcData
            {
                Colors = colors,
                NameId = Shader.PropertyToID(this.BufferName),
            });
        }

    }


    public static partial class ColorPalletUtiity
    { 
        public static ColorPalletBuilder GetColorPalletBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetExistingSystem<ColorPalletShaderBufferConversion>().Pallets;
        }
    }
}
