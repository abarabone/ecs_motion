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

using Abarabone.Authoring;
using Abarabone.CharacterMotion;
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

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>( isReadOnly: true );
            var drawTargets = this.GetComponentDataFromEntity<DrawInstance.TargetWorkData>(isReadOnly: true);

            inputDeps = this.Entities
                .WithBurst()
                .WithNone<NonUniformScale>()
                .WithReadOnly( offsetsOfDrawModel )
                .WithReadOnly(drawTargets)
                .ForEach(
                    (
                        in DrawTransform.LinkData linkerOfTf,
                        in DrawTransform.IndexData indexerOfTf,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        var drawtarget = drawTargets[linkerOfTf.DrawInstanceEntity];
                        if (drawtarget.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linkerOfTf.DrawModelEntityCurrent];

                        const int vectorLengthInBone = 2;
                        var lengthOfInstance = vectorLengthInBone * indexerOfTf.BoneLength + offsetInfo.VectorOffsetPerInstance;
                        var boneOffset = drawtarget.DrawInstanceId * lengthOfInstance;
                        var i = boneOffset + vectorLengthInBone * indexerOfTf.BoneId + offsetInfo.VectorOffsetPerInstance;

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                        pModel[ i + 0 ] = new float4( pos.Value, 1.0f );
                        pModel[ i + 1 ] = rot.Value.value;
                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }



    }

}
