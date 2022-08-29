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

    /// <summary>
    /// 
    /// </summary>
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class UvPaletteShaderBufferConversion : GameObjectConversionSystem
    {


        public UvPaletteBufferBuilder Builder { get; } = new ();

        public string BufferName = "UvPalettes";



        protected override void OnUpdate()
        {
            var texdict = this.GetTextureAtlasDictionary();
            var uvrects = this.Builder.ToUvRectArray(texdict.);
            if (uvrects.Length == 0) return;


            var em = this.DstEntityManager;

            this.Entities
                .ForEach((Entity ent, UvPaletteBufferAuthoring auth) =>
                {
                    var buffer = auth.Palettes.BuildShaderBuffer();

                    em.AddComponentData(ent, new ShaderBuffer.ColorPaletteData
                    {
                        Buffer = buffer,
                        NameId = Shader.PropertyToID(auth.BufferName),
                    });
                });

            var ent = em.CreateEntity();

            em.AddComponentData(ent, new ShaderBuffer.ColorPaletteSrcData
            {
                Colors = colors,
                NameId = Shader.PropertyToID(this.BufferName),
            });
        }

    }

}
