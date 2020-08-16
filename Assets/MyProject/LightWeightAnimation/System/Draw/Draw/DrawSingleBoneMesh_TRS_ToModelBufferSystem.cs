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
using Abarabone.Geometry;

namespace Abarabone.Draw
{

    /// <summary>
    /// TRSだが、現在はTRのみ対応
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    [UpdateBefore( typeof( BeginDrawCsBarier ) )]
    public class DrawSingleBoneMesh_TRS_ToModelBufferSystem : SystemBase
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override void OnUpdate()
        {

            var modelBuffers = this.GetComponentDataFromEntity<DrawModel.VectorBufferData>( isReadOnly: true );
            var vectorLengths = this.GetComponentDataFromEntity<DrawModel.VectorLengthData>(isReadOnly: true);

            var dependency = this.Entities
                .WithBurst()
                .WithReadOnly(modelBuffers)
                .WithReadOnly(vectorLengths)
                .WithAll<DrawInstance.MeshTag>()
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModeLinkData linker,
                        in Translation pos,
                        in Rotation rot//,
                        //in NonUniformScale scl
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = modelBuffers[linker.DrawModelEntityCurrent];
                        var vcLength = vectorLengths[linker.DrawModelEntityCurrent];

                        const int vectorLengthInBone = 2;
                        var lengthOfInstance = vectorLengthInBone + vcLength.VectorLengthOfInstanceAdditionalData;// あとでスケールに対応させる
                        var i = target.DrawInstanceId * lengthOfInstance + vcLength.VectorLengthOfInstanceAdditionalData;

                        var pModel = offsetInfo.pVectorPerModelInBuffer;
                        pModel[i + 0] = new float4(pos.Value, 1.0f);
                        pModel[i + 1] = rot.Value.value;

                    }
                )
                .ScheduleParallel( this.Dependency );
            this.Dependency = dependency;


            this.presentationBarier.AddJobHandleForProducer( dependency );

        }



    }

}
