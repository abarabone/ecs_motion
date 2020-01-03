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


using Abss.Arthuring;
using Abss.Motion;
using Abss.SystemGroup;

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    //[UpdateBefore(typeof( BeginDrawCsBarier ) )]
    public class BoneToDrawInstanceSystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new BoneToDrawInstanceJob
            {
                modelIndexsOfDrawInstance = this.GetComponentDataFromEntity<DrawIndexOfModelData>( isReadOnly: true ),
                UnitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModelBoneUnitSizeData>( isReadOnly: true ),
                OffsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModelInstanceOffsetData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }


        [BurstCompile, ExcludeComponent(typeof(Scale))]
        struct BoneToDrawInstanceJob : IJobForEach
            <BoneDrawLinkData, BoneIndexData, BoneDrawTargetIndexWorkData, Translation, Rotation>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelBoneUnitSizeData> UnitSizesOfDrawModel;
            [ReadOnly]
            public ComponentDataFromEntity<DrawIndexOfModelData> modelIndexsOfDrawInstance;
            [ReadOnly]
            public ComponentDataFromEntity<DrawModelInstanceOffsetData> OffsetsOfDrawModel;


            public unsafe void Execute(
                [ReadOnly] ref BoneDrawLinkData linkerOfBone,
                [ReadOnly] ref BoneIndexData indexerOfBone,
                [ReadOnly] ref BoneDrawTargetIndexWorkData targetOfBone,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot
            )
            {

                var modelIndexer = this.modelIndexsOfDrawInstance[ linkerOfBone.DrawEntity ];

                var vectorLengthInBone = this.UnitSizesOfDrawModel[ modelIndexer.ModelEntity ].VectorLengthInBone;
                var i = targetOfBone.BoneOffsetInModelBuffer * vectorLengthInBone;

                var pInstance = this.OffsetsOfDrawModel[ modelIndexer.ModelEntity ].pVectorOffsetInBuffer;
                pInstance[ i + 0 ] = new float4( pos.Value, 1.0f );
                pInstance[ i + 1 ] = rot.Value.value;

            }
        }



    }

}
