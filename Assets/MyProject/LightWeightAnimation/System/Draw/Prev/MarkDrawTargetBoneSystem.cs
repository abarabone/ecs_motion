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

namespace Abarabone.Draw
{
    
    /// <summary>
    /// 描画対象ボーンのマークを兼ね、モデル内描画位置をセットする。
    /// </summary>
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    [UpdateAfter( typeof( DrawCullingSystem ) )]
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
            var offsets = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>(isReadOnly: true);

            this.Entities
            //var withLod = this.Entities
                .WithBurst()
                //.WithAll<DrawTransform.WithLodTag>()
                .WithReadOnly(modelLinks)
                .WithReadOnly(targets)
                .ForEach(
                    (
                        //ref DrawTransform.TargetWorkData boneIndexer,
                        ref DrawTransform.VectorBufferData buffer,
                        in DrawTransform.LinkData drawLinker,
                        in DrawTransform.IndexData boneInfo
                    ) =>
                    {

                        var drawTarget = targets[drawLinker.DrawInstanceEntity];

                        var currentModelEntity = modelLinks[drawLinker.DrawInstanceEntity].DrawModelEntityCurrent;
                        var offset = offsets[currentModelEntity];


                        var vectorLengthOfInstance = boneInfo.VectorLengthInBone * boneInfo.BoneLength + offset.VectorOffsetPerInstance;
                        var vectorOffsetOfInstance = drawTarget.DrawInstanceId * vectorLengthOfInstance;

                        var i = vectorOffsetOfInstance + boneInfo.VectorLengthInBone * boneInfo.BoneId + offset.VectorOffsetPerInstance;


                        buffer.pVectorPerBoneInBuffer = offset.pVectorPerModelInBuffer + i;
                    }
                )
                .ScheduleParallel();
        }

        
    }

}
