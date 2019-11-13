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
                StreamValues = this.GetComponentDataFromEntity<StreamInterpolatedData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile]
        public struct StreamToBoneJob : IJobForEach
            //<BoneStreamLinkData, Translation, Rotation>
            <BoneStreamLinkBlend2Data, BoneLocalValueData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<StreamInterpolatedData> StreamValues;


            public void Execute(
                [ReadOnly]  ref BoneStreamLinkBlend2Data streamLinker,
                [WriteOnly] ref BoneLocalValueData local
            )
            {

                var pos0 = this.StreamValues[ streamLinker.PositionStream0Entity ].Value.As_float3();
                var rot0 = this.StreamValues[ streamLinker.RotationStream0Entity ].Value;

                var pos1 = this.StreamValues[ streamLinker.PositionStream1Entity ].Value.As_float3();
                var rot1 = this.StreamValues[ streamLinker.RotationStream1Entity ].Value;

                var wei0 = streamLinker.weight0;
                var wei1 = 1.0f - streamLinker.weight0;

                local.Position = pos0 * wei0 + pos1 * wei1;
                local.Rotation = rot0 * wei0 + rot1 * wei1;
                
            }
        }

    }

}
