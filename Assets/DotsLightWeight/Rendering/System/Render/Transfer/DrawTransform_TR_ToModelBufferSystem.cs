﻿using System.Collections;
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


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class DrawTransform_TR_ToModelBufferSystem : DependsDrawCsSystemBase
    {

        //BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        //protected override void OnStartRunning()
        //{
        //    this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        //}


        protected unsafe override void OnUpdateWith()
        {
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithNone<NonUniformScale>()
                .WithReadOnly(offsetsOfDrawModel)
                .ForEach(
                    (
                        in BoneDraw.LinkData linker,
                        in BoneDraw.IndexData indexer,
                        in BoneDraw.TargetWorkData target,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        const int vectorLengthInBone = 2;
                        var lengthOfInstance = vectorLengthInBone * indexer.BoneLength + offsetInfo.VectorOffsetPerInstance;
                        var boneOffset = target.DrawInstanceId * lengthOfInstance;
                        var i = boneOffset + vectorLengthInBone * indexer.BoneId + offsetInfo.VectorOffsetPerInstance;

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                        pModel[i + 0] = new float4(pos.Value, 1.0f);
                        pModel[i + 1] = rot.Value.value;
                    }
                )
                .ScheduleParallel();

            //this.presentationBarier.AddJobHandleForProducer(this.Dependency);
        }
    }

}
