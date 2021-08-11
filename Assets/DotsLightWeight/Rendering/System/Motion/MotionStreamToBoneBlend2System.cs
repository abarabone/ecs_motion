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


namespace DotsLite.CharacterMotion
{

    
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.Model;


    //[DisableAutoCreation]
    [UpdateAfter( typeof( MotionStreamProgressAndInterporationSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Render.Draw.Transform.MotionBone ) )]
    public class StreamToBoneBlend2System : SystemBase
    {


        protected override void OnUpdate()
        {

            var blends = this.GetComponentDataFromEntity<MotionBlend2WeightData>(isReadOnly: true);
            var streamValues = this.GetComponentDataFromEntity<Stream.InterpolationData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithNone<Bone.Stream2LinkData>()
                .WithReadOnly(blends)
                .WithReadOnly(streamValues)
                .ForEach(
                    (
                        ref Bone.LocalValueData local,
                        in Bone.MotionBlendLinkData linker,
                        in Bone.Stream0LinkData stream0Linker,
                        in Bone.Stream1LinkData stream1Linker
                    )
                =>
                    {
                        var pos0 = streamValues[stream0Linker.PositionStreamEntity].Interpolation.As_float3();
                        var rot0 = streamValues[stream0Linker.RotationStreamEntity].Interpolation.As_quaternion();

                        var pos1 = streamValues[stream1Linker.PositionStreamEntity].Interpolation.As_float3();
                        var rot1 = streamValues[stream1Linker.RotationStreamEntity].Interpolation.As_quaternion();

                        var blendWeight = blends[linker.MotionBlendEntity];

                        var wei0 = blendWeight.WeightNormalized0;
                        var wei1 = blendWeight.WeightNormalized1;

                        local.Position = pos0 * wei0 + pos1 * wei1;
                        local.Rotation = math.slerp(rot1, rot0, wei0);
                    }
                )
                .ScheduleParallel();
        }

    }

}
