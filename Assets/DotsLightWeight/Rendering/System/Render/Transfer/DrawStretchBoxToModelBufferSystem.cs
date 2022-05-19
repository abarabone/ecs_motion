using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

namespace DotsLite.Draw
{
    using DotsLite.Dependency;

    /// <summary>
    /// TRSだが、現在はTRのみ対応
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
    public partial class DrawStretchBoxToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawBufferToShaderDataSystem>(this);
        }


        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithReadOnly(nativeBuffers)
                .WithAll<DrawInstance.MeshTag>()
                .WithNone<DrawInstance.BoneModelTag>()
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Translation pos,
                        in Rotation rot,
                        in StretchBox.SizeData size,
                        in StretchBox.UvIndexData iuv,
                        in Palette.PaletteData ipalette
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        var lengthOfInstance = 4 + offsetInfo.OptionalVectorLengthPerInstance;
                        var i = target.DrawInstanceId * lengthOfInstance + offsetInfo.OptionalVectorLengthPerInstance;

                        var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                        pModel[i + 0] = new float4(pos.Value, 1.0f);
                        pModel[i + 1] = rot.Value.value;
                        pModel[i + 2] = size.Size;
                        //pModel[i + 3] = new float4(iuv.as_float2, ipalette.as_float2);
                    }
                )
                .ScheduleParallel();
        }



    }

}
