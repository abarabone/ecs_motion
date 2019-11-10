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
using Abss.SystemGroup;
using Abss.Utilities;
using Abss.Geometry;
using Abss.Character;

namespace Abss.Motion
{

    [UpdateAfter( typeof( MotionStreamProgressAndInterporationSystem ) )]
    [UpdateInGroup( typeof( MotionSystemGroup ) )]
    public class StreamToBoneSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new StreamToBoneJob
            {
                StreamValues = this.GetComponentDataFromEntity<StreamInterpolatedData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile]
        public struct StreamToBoneJob : IJobForEach<BoneStreamLinkData, Translation, Rotation>
        {

            [ReadOnly]
            public ComponentDataFromEntity<StreamInterpolatedData> StreamValues;


            public void Execute(
                [ReadOnly]  ref BoneStreamLinkData streamLinker,
                [WriteOnly] ref Translation pos,
                [WriteOnly] ref Rotation rot
            )
            {

                pos.Value = this.StreamValues[ streamLinker.PositionStreamEntity ].Value.As_float3();
                rot.Value = this.StreamValues[ streamLinker.RotationStreamEntity ].Value;

            }
        }

    }

}
