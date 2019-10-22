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
using Abss.Misc;

namespace Abss.Draw
{

    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    //[UpdateAfter(typeof( DrawCullingDummySystem ) )]
    ////[UpdateAfter( typeof( DrawPrevSystemGroup ) )]
    ////[UpdateBefore(typeof(DrawSystemGroup))]
    //[UpdateInGroup( typeof( DrawPrevSystemGroup ) )]
    public class DrawInstanceTempBufferAllocationSystem : JobComponentSystem
    {



        // 一括ボーンフレームバッファ
        public NativeArray<float4> TempInstanceBoneVectors { get; private set; }




        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            if( this.TempInstanceBoneVectors.IsCreated )
                this.TempInstanceBoneVectors.Dispose();



            return inputDeps;
        }

        protected override void OnDestroy()
        {
            if( this.TempInstanceBoneVectors.IsCreated )
                this.TempInstanceBoneVectors.Dispose();
        }




        struct DrawInstanceTempBufferAllocationJob : IJob
        {

            public NativeArray<ThreadSafeCounter<Persistent>> InstanceCounters;


            public NativeArray<float4> InstanceVectorBuffer;


            public void Execute()
            {

                var length = 0;
                foreach( var x in this.InstanceCounters )
                {

                    length += x.Count;

                }

            }
        }
    }
}
