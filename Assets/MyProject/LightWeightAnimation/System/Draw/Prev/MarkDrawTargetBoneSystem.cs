using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;


using Abarabone.Authoring;
using Abarabone.CharacterMotion;
using Abarabone.SystemGroup;
using Abarabone.Model;

namespace Abarabone.Draw
{
    
    /// <summary>
    /// 描画対象ボーンのマークを兼ね、モデル内描画位置をセットする。
    /// </summary>
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    [UpdateAfter( typeof( DrawCullingSystem ) )]
    [UpdateAfter(typeof(DrawInstanceTempBufferAllocateSystem))]//
    public class MarkDrawTargetBoneSystem : SystemBase
    {


        protected override unsafe void OnUpdate()
        {

            var targets = this.GetComponentDataFromEntity<DrawInstance.TargetWorkData>(isReadOnly: true);

            ////var withoutLod = this.Entities
            //this.Entities
            //    .WithBurst()
            //    .WithNone<DrawTransform.WithLodTag>()
            //    .WithReadOnly(targets)
            //    .ForEach(
            //        (
            //            ref DrawTransform.TargetWorkData boneIndexer,
            //            in DrawTransform.LinkData drawLinker,
            //            in DrawTransform.IndexData boneId
            //        ) =>
            //        {

            //            var drawTarget = targets[drawLinker.DrawInstanceEntity];

            //            boneIndexer.DrawInstanceId = drawTarget.DrawInstanceId;

            //        }
            //    )
            //    .ScheduleParallel();


            var modelLinks = this.GetComponentDataFromEntity<DrawInstance.ModeLinkData>(isReadOnly: true);
            var modelBuffers = this.GetComponentDataFromEntity<DrawModel.VectorBufferData>(isReadOnly: true);
            var vectorLengths = this.GetComponentDataFromEntity<DrawModel.VectorLengthData>(isReadOnly: true);

            this.Entities
            //var withLod = this.Entities
                .WithBurst()
                //.WithAll<DrawTransform.WithLodTag>()
                .WithReadOnly(modelLinks)
                .WithReadOnly(targets)
                .WithReadOnly(modelBuffers)
                .WithReadOnly(vectorLengths)
                .ForEach(
                    (
                        ref DrawTransform.VectorBufferData buffer,
                        in DrawTransform.LinkData drawLinker,
                        in DrawTransform.IndexData boneInfo
                    ) =>
                    {

                        var drawTarget = targets[drawLinker.DrawInstanceEntity];

                        if (drawTarget.DrawInstanceId == -1)
                        {
                            buffer.pVectorPerBoneInBuffer = null;
                            buffer._pVectorPerBoneInBuffer = (int)buffer.pVectorPerBoneInBuffer;
                            return;
                        }

                        var currentModelEntity = modelLinks[drawLinker.DrawInstanceEntity].DrawModelEntityCurrent;
                        var vcLength = vectorLengths[currentModelEntity];
                        var modelBuffer = modelBuffers[currentModelEntity];

                        var vectorOffsetOfInstance = drawTarget.DrawInstanceId * vcLength.VecotrLengthPerInstance;
                        var vectorOffsetOfBone = boneInfo.VectorBufferOffsetOfBone;
                        var i = vectorOffsetOfInstance + vectorOffsetOfBone;

                        buffer.pVectorPerBoneInBuffer = modelBuffer.pVectorPerModelInBuffer + i;
                        buffer._pVectorPerBoneInBuffer = (int)buffer.pVectorPerBoneInBuffer;
                    }
                )
                //.Schedule();
                .ScheduleParallel();
        }

        
    }

}
