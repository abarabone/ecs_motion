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


using Abarabone.CharacterMotion;
using Abarabone.SystemGroup;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.Draw
{

    using Abarabone.Structure;
    using Abarabone.Dependency;


    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class StructureParameterToModelBufferSystem : DependencyAccessableSystemBase
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
        }



    }

}
