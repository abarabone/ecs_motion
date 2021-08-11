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


using DotsLite.CharacterMotion;
using DotsLite.SystemGroup;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Draw
{

    using DotsLite.Structure;
    using DotsLite.Dependency;
    using DotsLite.Model;


    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Render.Draw.Transfer) )]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    //[UpdateBefore(typeof(DrawMeshCsSystem))]
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
                        in DrawInstance.ModelLinkData linker,
                        in Main.PartDestructionData destruction
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];
                        //var boneInfo = boneinfoOfDrawModel[linker.DrawModelEntityCurrent];

                        var pDstBase = offsetInfo.pVectorOffsetPerModelInBuffer;
                        var boneVectorLength = (int)BoneType.RT;//boneInfo.VectorLengthInBone * boneInfo.BoneLength;
                        var instanceVectorLength = boneVectorLength + offsetInfo.VectorOffsetPerInstance;

                        var i = target.DrawInstanceId * instanceVectorLength;
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
