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

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Render.Draw.Transfer ) )]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
    public partial class DrawSingleBoneMeshWithColorPalette_TR_ToModelBufferSystem : DependencyAccessableSystemBase
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

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>( isReadOnly: true );

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithReadOnly(nativeBuffers)
                .WithAll<DrawInstance.MeshTag>()
                .WithNone<DrawInstance.BoneModelTag>()
                .WithAll<DrawInstance.TransferSpecialTag>()
                .WithNone<NonUniformScale>()
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Translation pos,
                        in Rotation rot,
                        in Draw.Palette.ColorPaletteData palette
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        var lengthOfInstance = 2 + offsetInfo.OptionalVectorLengthPerInstance;
                        var i = target.DrawInstanceId * lengthOfInstance + offsetInfo.OptionalVectorLengthPerInstance;

                        var pid_base = math.asfloat(palette.BaseIndex);

                        var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                        pModel[i + 0] = new float4(pos.Value, pid_base);
                        pModel[i + 1] = rot.Value.value;

                    }
                )
                .ScheduleParallel();
        }



    }

}
