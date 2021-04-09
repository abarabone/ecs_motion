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

//
//using Abarabone.CharacterMotion;
//using Abarabone.SystemGroup;

namespace Abarabone.Draw
{
    using Abarabone.Dependency;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class DrawTransform_T_ToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawMeshCsSystem>(this);
        }

        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>(isReadOnly: true);
            
            this.Entities
                .WithBurst()
                .WithNone<NonUniformScale, Rotation>()
                .WithReadOnly(offsetsOfDrawModel)
                .ForEach(
                    (
                        in BoneDraw.LinkData linker,
                        in BoneDraw.IndexData indexer,
                        in BoneDraw.TargetWorkData target,
                        in Translation pos
                    )
                =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        const int vectorLengthInBone = 1;
                        var lengthOfInstance = vectorLengthInBone * indexer.BoneLength + offsetInfo.VectorOffsetPerInstance;
                        var boneOffset = target.DrawInstanceId * lengthOfInstance;
                        var i = boneOffset + vectorLengthInBone * indexer.BoneId + offsetInfo.VectorOffsetPerInstance;

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                        pModel[i] = new float4(pos.Value, 1.0f);
                    }
                )
                .ScheduleParallel();
        }
    }

}
