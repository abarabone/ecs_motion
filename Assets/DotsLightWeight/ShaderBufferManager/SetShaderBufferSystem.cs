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
using UnityEngine;

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

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            var em = this.EntityManager;

            this.Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity ent, ShaderBuffer.ColorPaletteSrcData src) =>
                {
                    Debug.Log("cre");
                    em.AddComponentData(ent, new ShaderBuffer.ColorPaletteData
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
                    DrawModelShaderBuffer.ColorPaletteLinkData setup) =>
                {
                    var buf = em.GetComponentData<ShaderBuffer.ColorPaletteData>(setup.BufferEntity);

                    geom.Material.SetBuffer(buf.NameId, buf.Buffer);
                })
                .Run();
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();

            this.Entities
                .WithoutBurst()
                //.ForEach((ShaderBuffer.BufferData buf) => buf.Buffer.Dispose())
                .ForEach((ShaderBuffer.ColorPaletteData buf) =>
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
