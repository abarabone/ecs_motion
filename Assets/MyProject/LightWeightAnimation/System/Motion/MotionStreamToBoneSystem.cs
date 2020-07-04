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


namespace Abarabone.Motion
{
    
    using Abarabone.Authoring;
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Model;


    [UpdateAfter( typeof( MotionStreamProgressAndInterporationSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
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



        [BurstCompile, ExcludeComponent( typeof( Bone.Stream1LinkData ), typeof( Bone.Stream2LinkData ) )]
        public struct StreamToBoneJob : IJobForEach
            //<BoneStreamLinkData, Translation, Rotation>
            <Bone.Stream0LinkData, Bone.LocalValueData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<StreamInterpolatedData> StreamValues;


            public void Execute(
                [ReadOnly]  ref Bone.Stream0LinkData streamLinker,
                //[WriteOnly] ref Translation pos,
                //[WriteOnly] ref Rotation rot
                [WriteOnly] ref Bone.LocalValueData local
            )
            {

                //pos.Value = this.StreamValues[ streamLinker.PositionStreamEntity ].Value.As_float3();
                //rot.Value = this.StreamValues[ streamLinker.RotationStreamEntity ].Value;

                local.Position = this.StreamValues[ streamLinker.PositionStreamEntity ].Value.As_float3();
                local.Rotation = this.StreamValues[ streamLinker.RotationStreamEntity ].Value;

            }
        }

    }

}
