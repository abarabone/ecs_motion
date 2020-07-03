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
using Abarabone.Motion;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

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

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModelInstanceOffsetData>( isReadOnly: true );

            var dependency = this.Entities
                .WithReadOnly(offsetsOfDrawModel)
                .WithBurst()
                .ForEach(
                    (
                        in DrawInstanceTargetWorkData target,
                        in DrawInstanceModeLinkData linker,
                        in Translation pos,
                        in Rotation rot,
                        in NonUniformScale scl
                    ) =>
                    {

                        var i = target.DrawInstanceId * 2;// スケールに対応させる

                        var pInstance = offsetsOfDrawModel[linker.DrawModelEntity].pVectorOffsetInBuffer;
                        pInstance[i + 0] = new float4(pos.Value, 1.0f);
                        pInstance[i + 1] = rot.Value.value;

                    }
                )
                .ScheduleParallel( this.Dependency );
            this.Dependency = dependency;


            this.presentationBarier.AddJobHandleForProducer( dependency );

        }



    }

}
