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

using Abss.Cs;
using Abss.Arthuring;
using Abss.Motion;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateInGroup(typeof(DrawSystemGroup))]
    public class BoneToDrawInstanceSystem : JobComponentSystem
    {

        DrawMeshCsSystem drawSystem;
        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証
        

        protected override void OnStartRunning()
        {
            this.drawSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var instanceBoneVectorEveryModels = this.drawSystem.GetInstanceBoneVectorEveryModels();
            if( !instanceBoneVectorEveryModels.IsCreated ) return inputDeps;


            inputDeps = new BoneToDrawInstanceJob
            {
                DstInstanceBoneVectorEveryModels = instanceBoneVectorEveryModels,
            }
            .Schedule( this, inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }


        struct BoneToDrawInstanceJob : IJobForEach<BoneDrawTargetIndexData, Translation, Rotation>
        {

            [NativeDisableParallelForRestriction]
            public NativeArray<NativeSlice<float4>> DstInstanceBoneVectorEveryModels;

            public void Execute(
                [ReadOnly] ref BoneDrawTargetIndexData indexer,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot
            )
            {

                var i = ( indexer.InstanceBoneOffset + indexer.BoneId ) * 2;

                var dstInstances = this.DstInstanceBoneVectorEveryModels[ indexer.ModelIndex ];
                dstInstances[ i + 0 ] = new float4( pos.Value, 1.0f );
                dstInstances[ i + 1 ] = rot.Value.value;

            }
        }


        struct BoneToDrawInstanceJob_direct : IJobForEach<BoneDrawLinkData, BoneDrawTargetIndexData, Translation, Rotation>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;

            [WriteOnly]
            public NativeArray<NativeArray<float4>> DstInstances;


            public void Execute(
                [ReadOnly] ref BoneDrawLinkData drawLinker,
                [ReadOnly] ref BoneDrawTargetIndexData boneIndexer,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot
            )
            {
                var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

                var i = ( drawIndexer.instanceIndex * drawIndexer.BoneLength + boneIndexer.BoneId ) * 2;

                var dstInstances = this.DstInstances[ drawIndexer.modelIndex ];
                dstInstances[ i + 0 ] = new float4( pos.Value, 1.0f );
                dstInstances[ i + 1 ] = rot.Value.value;
            }
        }


    }

}
