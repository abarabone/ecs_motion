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
    using DotsLite.Geometry.Palette;


    /// <summary>
    /// パレットのもとになるデータ（XxxSrcData）から、グラフィックスバッファを生成初期化し、マテリアルにセットする。
    /// また、最後に破棄も行う。
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Call))]
    public partial class SetShaderBufferToMaterialSystem : SystemBase
    {

        protected override void OnStartRunning()
        {
            this.RequireSingletonForUpdate<ShaderBuffer.ColorPaletteSrcData>();
            base.OnStartRunning();

            var em = this.EntityManager;

            //this.Entities
            //    .WithoutBurst()
            //    .WithStructuralChanges()
            //    .ForEach((Entity ent, ShaderBuffer.ColorPaletteSrcData src) =>
            //    {
            //        Debug.Log("cre");
            //        em.AddComponentData(ent, new ShaderBuffer.ColorPaletteData
            //        {
            //            Buffer = src.Colors.BuildShaderBuffer(),
            //            NameId = src.NameId,
            //        });
            //    })
            //    .Run();

            if (!this.TryGetSingletonEntity<ShaderBuffer.ColorPaletteSrcData>(out var bufent)) return; ;
            Debug.Log("cre");

            //var bufent = this.GetSingletonEntity<ShaderBuffer.ColorPaletteSrcData>();
            var src = em.GetComponentData<ShaderBuffer.ColorPaletteSrcData>(bufent);
            var buf = new ShaderBuffer.ColorPaletteData
            {
                Buffer = src.Colors.BuildColorPaletteShaderBuffer(),
                NameId = src.NameId,
            };
            em.AddComponentData(bufent, buf);


            this.Entities
                .WithoutBurst()
                .ForEach((
                    Entity ent,
                    DrawModel.GeometryData geom,
                    DrawModelShaderBuffer.ColorPaletteLinkData setup) =>
                {
                    geom.Material.SetBuffer(buf.NameId, buf.Buffer);

                    setup.BufferEntity = bufent;
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
