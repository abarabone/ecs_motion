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

    //[DisableAutoCreation]
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof( DrawAllocationGroup ) )]
    public class DrawInstanceTempBufferAllocationSystem : JobComponentSystem
    {



        // 一括ボーンフレームバッファ
        public NativeArray<float4> TempInstanceBoneVectors { get; private set; }

        DrawMeshCsSystem drawMeshCsSystem;


        protected override void OnStartRunning()
        {
            this.drawMeshCsSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            if( !this.drawMeshCsSystem.NativeBuffers.Units.IsCreated )
                return inputDeps;

            if( this.TempInstanceBoneVectors.IsCreated )
                this.TempInstanceBoneVectors.Dispose();


            inputDeps = new DrawInstanceTempBufferAllocationJob
            {
                NativeInstances = this.drawMeshCsSystem.NativeBuffers.Units,
            }
            .Schedule( inputDeps );


            return inputDeps;
        }

        protected override void OnDestroy()
        {
            if( this.TempInstanceBoneVectors.IsCreated )
                this.TempInstanceBoneVectors.Dispose();
        }




        struct DrawInstanceTempBufferAllocationJob : IJob
        {

            [ReadOnly]
            public NativeArray<DrawInstanceNativeBufferUnit> NativeInstances;
            
            //public NativeArray<float4> InstanceVectorBuffer;


            public void Execute()
            {

                var length = 0;
                foreach( var x in this.NativeInstances )
                {
                    length += x.InstanceCounter.Count;
                }

                //Debug.Log( length );
            }
        }
    }
}
