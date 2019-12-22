using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;


using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateBefore( typeof( DrawCullingDummySystem ) )]
    [UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class DrawInstanceResetSystem : JobComponentSystem
    {

        EntityQuery instanceCounterQuery;


        protected override void OnCreate()
        {
            this.instanceCounterQuery = this.GetEntityQuery(
                ComponentType.ReadWrite<DrawModelInstanceCounterData>()
            );
        }

        protected override void OnDestroy()
        {
            this.Entities.wi(this.instanceCounterQuery)
                .for
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var nativeInstanceBuffers = this.drawSystem.NativeBuffers;
            if( !nativeInstanceBuffers.Units.IsCreated ) return inputDeps;


            nativeInstanceBuffers.Reset();

            inputDeps = new DrawCullingDummyJob
            {
                NativeBuffers = nativeInstanceBuffers.Units,
            }
            .Schedule( this, inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }


    }

}
