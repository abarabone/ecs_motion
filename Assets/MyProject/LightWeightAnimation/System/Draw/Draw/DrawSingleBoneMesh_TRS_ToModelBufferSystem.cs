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

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>( isReadOnly: true );

            var dependency = this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
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


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        var lengthOfInstance = 2 + offsetInfo.VectorOffsetPerInstance;// あとでスケールに対応させる
                        var i = target.DrawInstanceId * lengthOfInstance + offsetInfo.VectorOffsetPerInstance;

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
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
