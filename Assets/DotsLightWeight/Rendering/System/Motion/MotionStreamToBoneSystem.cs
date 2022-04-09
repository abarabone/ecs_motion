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
    using DotsLite.Draw;

    [UpdateAfter( typeof( MotionStreamProgressAndInterporationSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Render.Draw.Transform.MotionBone ) )]
    public partial class StreamToBoneSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var streamValues = this.GetComponentDataFromEntity<Stream.InterpolationData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(streamValues)
                .ForEach(
                    (
                        ref Bone.LocalValueData local,
                        in Bone.Stream0LinkData streamLinker,
                        in BoneDraw.TargetWorkData target
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        local.Position = streamValues[streamLinker.PositionStreamEntity].Interpolation.As_float3();
                        local.Rotation = streamValues[streamLinker.RotationStreamEntity].Interpolation;

                    }
                )
                .ScheduleParallel();

        }



        //[BurstCompile, ExcludeComponent( typeof( Bone.Stream1LinkData ), typeof( Bone.Stream2LinkData ) )]
        //public struct StreamToBoneJob : IJobForEach
        //    //<BoneStreamLinkData, Translation, Rotation>
        //    <Bone.Stream0LinkData, Bone.LocalValueData>
        //{

        //    [ReadOnly]
        //    public ComponentDataFromEntity<Stream.InterpolationData> StreamValues;


        //    public void Execute(
        //        [ReadOnly]  ref Bone.Stream0LinkData streamLinker,
        //        //[WriteOnly] ref Translation pos,
        //        //[WriteOnly] ref Rotation rot
        //        [WriteOnly] ref Bone.LocalValueData local
        //    )
        //    {

        //        //pos.Value = this.StreamValues[ streamLinker.PositionStreamEntity ].Value.As_float3();
        //        //rot.Value = this.StreamValues[ streamLinker.RotationStreamEntity ].Value;

        //        local.Position = this.StreamValues[ streamLinker.PositionStreamEntity ].Interpolation.As_float3();
        //        local.Rotation = this.StreamValues[ streamLinker.RotationStreamEntity ].Interpolation;

        //    }
        //}

    }

}
