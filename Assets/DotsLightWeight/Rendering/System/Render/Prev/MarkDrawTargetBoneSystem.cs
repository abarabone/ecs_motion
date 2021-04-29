using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;



using DotsLite.CharacterMotion;
using DotsLite.SystemGroup;

namespace DotsLite.Draw
{

    /// <summary>
    /// 描画対象ボーンのマークを兼ね、モデル内描画位置をセットする。
    /// </summary>
    ////[UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    ////[UpdateAfter( typeof( DrawCullingSystem ) )]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.Marking))]
    public class MarkDrawTargetBoneSystem : SystemBase
    {


        protected override void OnUpdate()
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


            var models = this.GetComponentDataFromEntity<DrawInstance.ModeLinkData>(isReadOnly: true);

            this.Entities
            //var withLod = this.Entities
                .WithBurst()
                //.WithAll<DrawTransform.WithLodTag>()
                .WithReadOnly(models)
                .WithReadOnly(targets)
                .ForEach(
                    (
                        ref BoneDraw.TargetWorkData boneIndexer,
                        ref BoneDraw.LinkData drawLinker,
                        in BoneDraw.IndexData boneId
                    ) =>
                    {

                        var drawTarget = targets[drawLinker.DrawInstanceEntity];

                        boneIndexer.DrawInstanceId = drawTarget.DrawInstanceId;


                        drawLinker.DrawModelEntityCurrent = models[drawLinker.DrawInstanceEntity].DrawModelEntityCurrent;
                    }
                    // ここでボーン書き込み位置を計算してしまうことも可能だが、そうすると DrawInstanceTempBufferAllocateSystem に依存する。
                    // ジョブの並列度が下がるので、ここでは最低限のことだけ行う。
                    // また、描画時にやるようにすればこのシステムは不要になるが、ボーン関係のシステム用にここでやっておく
                )
                .ScheduleParallel();
        }

        
    }

}
