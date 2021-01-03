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


namespace Abarabone.Draw
{
    using Abarabone.CharacterMotion;
    using Abarabone.SystemGroup;


    //[UpdateAfter( typeof( DrawInstanceTempBufferAllocationSystem ) )]
    static class A
    {

        //struct BoneToDrawInstanceJob_direct : IJobForEach<BoneDrawLinkData, BoneDrawTargetIndexWorkData, Translation, Rotation>
        //{

        //    [ReadOnly]
        //    public ComponentDataFromEntity<DrawModelIndexData> DrawIndexers;

        //    [WriteOnly]
        //    public NativeArray<NativeArray<float4>> DstInstances;


        //    public void Execute(
        //        [ReadOnly] ref BoneDrawLinkData drawLinker,
        //        [ReadOnly] ref BoneDrawTargetIndexWorkData boneIndexer,
        //        [ReadOnly] ref Translation pos,
        //        [ReadOnly] ref Rotation rot
        //    )
        //    {
        //        var drawIndexer = this.DrawIndexers[ drawLinker.DrawEntity ];

        //        var i = ( drawIndexer.InstanceIndex * drawIndexer.BoneLength + boneIndexer.BoneId ) * 2;

        //        var dstInstances = this.DstInstances[ drawIndexer.ModelIndex ];
        //        dstInstances[ i + 0 ] = new float4( pos.Value, 1.0f );
        //        dstInstances[ i + 1 ] = rot.Value.value;
        //    }
        //}

    }
}
