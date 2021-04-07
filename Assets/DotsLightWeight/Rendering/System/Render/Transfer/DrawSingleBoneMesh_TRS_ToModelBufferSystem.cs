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

namespace Abarabone.Draw
{
    using Abarabone.Dependency;

    /// <summary>
    /// TRSだが、現在はTRのみ対応
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class DrawSingleBoneMesh_TRS_ToModelBufferSystem : DependencyAccessableSystemBase
    {


        RegisterBarrier<DrawMeshCsSystem> bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = new RegisterBarrier<DrawMeshCsSystem>(this);
        }


        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>( isReadOnly: true );

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.MeshTag>()
                .WithNone<DrawInstance.BoneModelTag>()
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModeLinkData linker,
                        in Translation pos,
                        in Rotation rot//,
                        //in NonUniformScale scl
                        // ＴＲＳといいつつ、現状はＴＲ
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
                .ScheduleParallel();
        }



    }

}
