using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Draw
{

    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Dependency;
    using DotsLite.Geometry;


    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Call))]
    public partial class SetShaderBufferToMaterialSystem : SystemBase
    {

        protected override void OnCreate()
        {
            base.OnCreate();

            var em = this.EntityManager;

            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity ent, ShaderBuffer.ColorPalletSrcData src) =>
                {
                    em.AddComponentData(ent, new ShaderBuffer.ColorPalletData
                    {
                        Buffer = src.Colors.BuildShaderBuffer(),
                        NameId = src.NameId,
                    });
                })
                .Run();

            this.Entities
                .WithoutBurst()
                .ForEach((
                    Entity ent,
                    DrawModel.GeometryData geom,
                    DrawModelShaderBuffer.ColorPalletLinkData setup) =>
                {
                    var buf = em.GetComponentData<ShaderBuffer.ColorPalletData>(setup.BufferEntity);

                    geom.Material.SetBuffer(buf.NameId, buf.Buffer);
                })
                .Run();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Entities
                .WithoutBurst()
                //.ForEach((ShaderBuffer.BufferData buf) => buf.Buffer.Dispose())
                .ForEach((ShaderBuffer.ColorPalletData buf) =>
                {
                    Debug.Log("dispose shader buffer");
                    buf.Buffer.Dispose();
                })
                .Run();
        }

        protected override void OnUpdate()
        {

        }
    }
}
