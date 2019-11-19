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
    public class StreamToBoneBlend2System : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new StreamToBoneJob
            {
                Blends = this.GetComponentDataFromEntity<BoneBlend2WeightData>( isReadOnly: true ),
                StreamValues = this.GetComponentDataFromEntity<StreamInterpolatedData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile, ExcludeComponent(typeof(BoneStream2LinkData))]
        public struct StreamToBoneJob : IJobForEach
            //<BoneStreamLinkData, Translation, Rotation>
            <BoneMotionLinkData, BoneStream0LinkData, BoneStream1LinkData, BoneLocalValueData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<BoneBlend2WeightData> Blends;

            [ReadOnly]
            public ComponentDataFromEntity<StreamInterpolatedData> StreamValues;


            public void Execute(
                [ReadOnly]  ref BoneMotionLinkData linker,
                [ReadOnly]  ref BoneStream0LinkData stream0Linker,
                [ReadOnly]  ref BoneStream1LinkData stream1Linker,
                [WriteOnly] ref BoneLocalValueData local
            )
            {

                var pos0 = this.StreamValues[ stream0Linker.PositionStreamEntity ].Value.As_float3();
                var rot0 = this.StreamValues[ stream0Linker.RotationStreamEntity ].Value.As_quaternion();

                var pos1 = this.StreamValues[ stream1Linker.PositionStreamEntity ].Value.As_float3();
                var rot1 = this.StreamValues[ stream1Linker.RotationStreamEntity ].Value.As_quaternion();

                var blendWeight = this.Blends[ linker.MotionEntity ];

                var wei0 = blendWeight.WeightNormalized0;
                var wei1 = blendWeight.WeightNormalized1;

                local.Position = pos0 * wei0 + pos1 * wei1;
                local.Rotation = math.slerp( rot0, rot1, wei0 );

            }
        }

    }

}
