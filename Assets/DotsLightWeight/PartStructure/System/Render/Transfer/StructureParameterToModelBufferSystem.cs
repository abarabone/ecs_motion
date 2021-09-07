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


            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);
            //var boneinfoOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneVectorSettingData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithAll<DrawInstance.LodCurrentIsNearTag>()// 1フレーム遅れるので対策は必要
                .WithReadOnly(nativeBuffers)
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

                        if (offsetInfo.OptionalVectorLengthPerInstance == 0) return;// １フレーム分の対策
                        // ただし、far でテクスチャに part 情報を入れる場合は、下記が必要となる


                        var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;
                        var boneVectorLength = (int)BoneType.RT;//boneInfo.VectorLengthInBone * boneInfo.BoneLength;
                        var instanceVectorLength = boneVectorLength + offsetInfo.OptionalVectorLengthPerInstance;

                        var i = target.DrawInstanceId * instanceVectorLength;
                        //var size = offsetInfo.OptionalVectorLengthPerInstance * sizeof(float4);
                        //fixed (void* pSrc = destruction.Destructions)
                        //{
                        //    UnsafeUtility.MemCpy(pModel + i, pSrc, size);
                        //}
                        pModel[i + 0] = math.asfloat(destruction._values0);
                        pModel[i + 1] = math.asfloat(destruction._values1);
                        pModel[i + 2] = math.asfloat(destruction._values2);
                        pModel[i + 3] = math.asfloat(destruction._values3);
                    }
                )
                .ScheduleParallel();
        }



    }

}
