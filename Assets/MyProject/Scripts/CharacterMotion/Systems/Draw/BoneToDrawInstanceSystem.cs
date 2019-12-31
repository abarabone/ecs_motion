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


using Abss.Arthuring;
using Abss.Motion;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    //[UpdateBefore(typeof( BeginDrawCsBarier ) )]
    public class BoneToDrawInstanceSystem : JobComponentSystem
    {

        DrawMeshCsSystem drawSystem;
        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        DrawInstanceTempBufferAllocateSystem tempBufferSystem;//

        protected override void OnStartRunning()
        {
            this.drawSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
            this.tempBufferSystem = this.World.GetExistingSystem<DrawInstanceTempBufferAllocateSystem>();//
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var nativeInstanceBuffers = this.drawSystem.NativeBuffers;
            if( !nativeInstanceBuffers.Units.IsCreated ) return inputDeps;


            inputDeps = new BoneToDrawInstanceJob
            {
                DrawModelInfos = this.GetComponentDataFromEntity<DrawModelBoneUnitSizeData>( isReadOnly: true ),
                DrawModelOffsets = this.GetComponentDataFromEntity <DrawModelInstanceOffsetData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            
            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }


        [BurstCompile, ExcludeComponent(typeof(Scale))]
        struct BoneToDrawInstanceJob : IJobForEach
            <BoneDrawLinkData, BoneIndexData, BoneDrawTargetIndexWorkData, Translation, Rotation>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelBoneUnitSizeData> DrawModelInfos;
            [ReadOnly]
            public ComponentDataFromEntity<DrawModelInstanceOffsetData> DrawModelOffsets;


            public unsafe void Execute(
                [ReadOnly] ref BoneDrawLinkData linker,
                [ReadOnly] ref BoneIndexData indexer,
                [ReadOnly] ref BoneDrawTargetIndexWorkData target,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot
            )
            {

                var vectorLengthInBone = this.DrawModelInfos[ linker.DrawEntity ].VectorLengthInBone;
                var i = target.VectorOffsetInBuffer * vectorLengthInBone;

                var pInstance = this.DrawModelOffsets[ linker.DrawEntity ].pVectorOffsetInBuffer;
                pInstance[ i + 0 ] = new float4( pos.Value, 1.0f );
                pInstance[ i + 1 ] = rot.Value.value;

            }
        }



    }

}
