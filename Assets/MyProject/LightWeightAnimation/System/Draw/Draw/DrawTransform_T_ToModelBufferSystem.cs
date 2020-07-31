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
    public class DrawTransform_T_ToModelBufferSystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>( isReadOnly: true );


            inputDeps = this.Entities
                .WithBurst()
                .WithNone<NonUniformScale, Rotation>()
                .WithReadOnly( offsetsOfDrawModel )
                .ForEach(
                    (
                        in DrawTransform.LinkData linkerOfTf,
                        in DrawTransform.IndexData indexerOfTf,
                        in DrawTransform.TargetWorkData targetOfTf,
                        in Translation pos
                    ) =>
                    {
                        var offsetInfo = offsetsOfDrawModel[linkerOfTf.DrawModelEntity];

                        const int vectorLengthInBone = 1;
                        var lengthOfInstance = vectorLengthInBone * indexerOfTf.BoneLength + offsetInfo.VectorOffsetPerInstance;
                        var boneOffset = targetOfTf.DrawInstanceId * lengthOfInstance;
                        var i = boneOffset + vectorLengthInBone * indexerOfTf.BoneId + offsetInfo.VectorOffsetPerInstance;

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                        pModel[ i ] = new float4( pos.Value, 1.0f );
                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }



    }

}
