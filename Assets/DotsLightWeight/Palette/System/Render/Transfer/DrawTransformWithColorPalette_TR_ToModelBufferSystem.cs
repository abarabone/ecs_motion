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

//
//using Abarabone.CharacterMotion;
//using Abarabone.SystemGroup;

namespace DotsLite.Draw
{
    using DotsLite.Dependency;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
    public partial class DrawTransformWithColorPalette_TR_ToModelBufferSystem : DependencyAccessableSystemBase
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
                .WithNone<NonUniformScale>()
                .WithAll<DrawInstance.TransSpecialferTag>()
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(offsetsOfDrawModel)
                .ForEach(
                    (
                        in BoneDraw.LinkData linker,
                        in BoneDraw.IndexData indexer,
                        in BoneDraw.TargetWorkData target,
                        in Translation pos,
                        in Rotation rot,
                        in Draw.Palette.ColorPaletteData palette
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        const int vectorLengthInBone = 2;
                        var instanceVectorLength = vectorLengthInBone * indexer.BoneLength + offsetInfo.OptionalVectorLengthPerInstance;
                        var instanceStart = target.DrawInstanceId * instanceVectorLength;
                        var i = instanceStart + vectorLengthInBone * indexer.BoneId + offsetInfo.OptionalVectorLengthPerInstance;

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
