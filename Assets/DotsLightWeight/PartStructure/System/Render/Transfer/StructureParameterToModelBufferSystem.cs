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


using Abarabone.CharacterMotion;
using Abarabone.SystemGroup;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.Draw
{

    using Abarabone.Structure;


    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    [UpdateBefore( typeof( BeginDrawCsBarier ) )]
    public class StructureParameterToModelBufferSystem : SystemBase
    {

        BeginDrawCsBarier presentationBarier;

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override void OnUpdate()
        {

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>(isReadOnly: true);
            //var boneinfoOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneVectorSettingData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                //.WithAll<Structure.ShowNearTag>()
                .WithReadOnly( offsetsOfDrawModel )
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModeLinkData linker,
                        in Structure.PartDestructionData destruction
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];
                        //var boneInfo = boneinfoOfDrawModel[linker.DrawModelEntityCurrent];

                        var pDstBase = offsetInfo.pVectorOffsetPerModelInBuffer;
                        var boneVectorLength = 2;//boneInfo.VectorLengthInBone * boneInfo.BoneLength;
                        var i = target.DrawInstanceId * (boneVectorLength + offsetInfo.VectorOffsetPerInstance);
                        var size = offsetInfo.VectorOffsetPerInstance * sizeof(float4);
                        fixed (void* pSrc = destruction.Destructions)
                        {
                            UnsafeUtility.MemCpy(pDstBase + i, pSrc, size);
                        }
                    }
                )
                .ScheduleParallel();


            this.presentationBarier.AddJobHandleForProducer(this.Dependency);
        }



    }

}
