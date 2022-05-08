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

    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class PalletShaderBufferConversion : GameObjectConversionSystem
    {

        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .ForEach((Entity ent, ColorPalletBufferAuthoring auth) =>
                {
                    var buffer = auth.Pallets.BuildShaderBuffer();

                    em.AddComponentData(ent, new ShaderBuffer.ColorPalletData
                    {
                        Buffer = buffer,
                        NameId = Shader.PropertyToID(auth.BufferName),
                    });
                });
        }

    }

}
