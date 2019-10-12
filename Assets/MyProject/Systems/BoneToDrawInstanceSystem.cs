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

namespace Abss.Draw
{

    public class BoneToDrawInstanceSystem : JobComponentSystem
    {


        //EntityQuery query;


        protected override void OnCreate()
        {
            var query = this.GetEntityQuery(
                new EntityQueryDesc
                {
                    All = new con
                }
            );
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //throw new System.NotImplementedException();

            return inputDeps;
        }


        struct BoneToDrawInstanceJob : IJobForEach<BoneDrawTargetIndexData, Translation, Rotation>
        {

            [WriteOnly]
            public NativeArray<NativeArray<(float4 pos, float4 rot)>> DstInstances;

            public void Execute
                ([ReadOnly] ref BoneDrawTargetIndexData indexer, [ReadOnly] ref Translation pos, [ReadOnly] ref Rotation rot )
            {
                var instances = this.DstInstances[ indexer.ModelIndex ];
                instances[ indexer.InstanceBoneIndex ] = (new float4( pos.Value, 1.0f ), rot.Value.value);
            }
        }


        struct BoneToDrawInstanceJob_direct : IJobForEach<BoneDrawLinkData, BoneDrawTargetIndexData, Translation, Rotation>
        {

            [ReadOnly]
            public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;

            [WriteOnly]
            public NativeArray<NativeArray<(float4 pos, float4 rot)>> DstInstances;


            public void Execute
                ( [ReadOnly] ref BoneDrawLinkData drawLinker, [ReadOnly] ref BoneDrawTargetIndexData boneIndexer, [ReadOnly] ref Translation pos, [ReadOnly] ref Rotation rot )
            {
                var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

                var i = drawIndexer.instanceIndex * drawIndexer.BoneLength + boneIndexer.BoneId;

                var instances = this.DstInstances[ drawIndexer.modelIndex ];
                instances[i] = (new float4( pos.Value, 1.0f ), rot.Value.value);
            }
        }


    }

}
