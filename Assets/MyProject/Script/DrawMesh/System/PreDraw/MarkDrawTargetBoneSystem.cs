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
using Abarabone.Motion;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{
    
    /// <summary>
    /// 描画対象ボーンのマークを兼ね、モデル内描画位置をセットする。
    /// </summary>
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    [UpdateAfter( typeof( DrawCullingDummySystem ) )]
    public class MarkDrawTargetBoneSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new MarkBoneJob
            {
                //DrawIndexers = this.GetComponentDataFromEntity<DrawModelIndexData>( isReadOnly: true ),
                DrawTargets = this.GetComponentDataFromEntity<DrawInstanceTargetWorkData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            //var instanceOffestsOfDrawModel =
            //    this.GetComponentDataFromEntity<DrawModelInstanceOffsetData>( isReadOnly: true );
            //var targetsOfDrawInstance =
            //    this.GetComponentDataFromEntity<DrawInstanceTargetWorkData>( isReadOnly: true );

            //this.Entities
            //    .WithBurst()
            //    .WithReadOnly(instanceOffestsOfDrawModel)
            //    .WithReadOnly(targetsOfDrawInstance)
            //    .ForEach(
            //        (
            //            ref BoneDrawTargetIndexWorkData indexOfBone,
            //            in BoneDrawLinkData drawLinkerOfBone,
            //            in BoneIndexData boneIdOfBone
            //        ) =>
            //        {

            //            var drawTarget = targetsOfDrawInstance[ drawLinkerOfBone.DrawEntity ];

            //            var a = instanceOffestsOfDrawModel[]

            //            indexOfBone.pBoneInBuffer = 

            //        }
            //    );

            return inputDeps;
        }


        [BurstCompile]
        struct MarkBoneJob : IJobForEach<DrawTransformLinkData, DrawTransformIndexData, DrawTransformTargetWorkData>
        {

            //[ReadOnly]
            //public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;
            [ReadOnly]
            public ComponentDataFromEntity<DrawInstanceTargetWorkData> DrawTargets;

            public void Execute(
                [ReadOnly] ref DrawTransformLinkData drawLinker,
                [ReadOnly] ref DrawTransformIndexData boneId,
                ref DrawTransformTargetWorkData boneIndexer
            )
            {

                //var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];
                var drawTarget = this.DrawTargets[ drawLinker.DrawInstanceEntity ];

                boneIndexer.DrawInstanceId = drawTarget.DrawInstanceId;

            }
        }
        
    }

}
