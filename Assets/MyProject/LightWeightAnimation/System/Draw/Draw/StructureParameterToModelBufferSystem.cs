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
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.Draw
{

    using Abarabone.Structure;


    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    [UpdateBefore( typeof( BeginDrawCsBarier ) )]
    public class StructureParameterToModelBufferSystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected unsafe override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>( isReadOnly: true );


            inputDeps = this.Entities
                .WithBurst()
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

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                        var i = target.DrawInstanceId * (2 + offsetInfo.VectorOffsetPerInstance);
                        var size = offsetInfo.VectorOffsetPerInstance * sizeof(float4);
                        fixed (void* pSrc = destruction.Destructions)
                        {
                            UnsafeUtility.MemCpy(pModel + i, pSrc, size);
                        }
                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }



    }

}
