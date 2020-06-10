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

using Abarabone.Arthuring;
using Abarabone.Motion;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    [UpdateBefore( typeof( BeginDrawCsBarier ) )]
    public class DrawTransform_TR_ToModelBufferSystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModelInstanceOffsetData>( isReadOnly: true );

            inputDeps = this.Entities
                .WithNone<Scale>()
                .WithReadOnly( offsetsOfDrawModel )
                .WithBurst()
                .ForEach(
                    (
                        in DrawTransformLinkData linkerOfTf,
                        in DrawTransformIndexData indexerOfTf,
                        in DrawTransformTargetWorkData targetOfTf,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        const int vectorLengthInBone = 2;
                        var bondOffset = targetOfTf.DrawInstanceId * indexerOfTf.BoneLength + indexerOfTf.BoneId;
                        var i = bondOffset * vectorLengthInBone;

                        var pInstance = offsetsOfDrawModel[ linkerOfTf.DrawModelEntity ].pVectorOffsetInBuffer;
                        pInstance[ i + 0 ] = new float4( pos.Value, 1.0f );
                        pInstance[ i + 1 ] = rot.Value.value;
                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }



    }

}
