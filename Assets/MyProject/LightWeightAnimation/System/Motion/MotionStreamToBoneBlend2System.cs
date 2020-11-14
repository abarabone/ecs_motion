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


namespace Abarabone.CharacterMotion
{

    
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Model;


    //[DisableAutoCreation]
    [UpdateAfter( typeof( MotionStreamProgressAndInterporationSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup ) )]
    public class StreamToBoneBlend2System : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            //inputDeps = new StreamToBoneJob
            //{
            //    Blends = this.GetComponentDataFromEntity<MotionBlend2WeightData>( isReadOnly: true ),
            //    StreamValues = this.GetComponentDataFromEntity<Stream.InterpolationData>( isReadOnly: true ),
            //}
            //.Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile, ExcludeComponent(typeof(Bone.Stream2LinkData))]
        public struct StreamToBoneJob : IJobForEach
            <Bone.MotionBlendLinkData, Bone.Stream0LinkData, Bone.Stream1LinkData, Bone.LocalValueData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<MotionBlend2WeightData> Blends;

            [ReadOnly]
            public ComponentDataFromEntity<Stream.InterpolationData> StreamValues;


            public void Execute(
                [ReadOnly]  ref Bone.MotionBlendLinkData linker,
                [ReadOnly]  ref Bone.Stream0LinkData stream0Linker,
                [ReadOnly]  ref Bone.Stream1LinkData stream1Linker,
                [WriteOnly] ref Bone.LocalValueData local
            )
            {

                var pos0 = this.StreamValues[ stream0Linker.PositionStreamEntity ].Interpolation.As_float3();
                var rot0 = this.StreamValues[ stream0Linker.RotationStreamEntity ].Interpolation.As_quaternion();

                var pos1 = this.StreamValues[ stream1Linker.PositionStreamEntity ].Interpolation.As_float3();
                var rot1 = this.StreamValues[ stream1Linker.RotationStreamEntity ].Interpolation.As_quaternion();

                var blendWeight = this.Blends[ linker.MotionBlendEntity ];

                var wei0 = blendWeight.WeightNormalized0;
                var wei1 = blendWeight.WeightNormalized1;

                local.Position = pos0 * wei0 + pos1 * wei1;
                local.Rotation = math.slerp( rot1, rot0, wei0 );

            }
        }

    }

}
