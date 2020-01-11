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

using Abss.Arthuring;
using Abss.Motion;
using Abss.SystemGroup;
using Abss.Geometry;
using Abss.Character;

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    [UpdateBefore( typeof( BeginDrawCsBarier ) )]
    public class LineParticleToDrawInstanceSystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected override unsafe JobHandle OnUpdate( JobHandle inputDeps )
        {

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModelInstanceOffsetData>( isReadOnly: true );

            var drawInstanceTargets = this.GetComponentDataFromEntity<DrawInstanceTargetWorkData>( isReadOnly: true );


            inputDeps = this.Entities
                //.WithoutBurst()
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .ForEach(
                    (
                        in LineParticlePointNodeData node,
                        in Translation pos
                    ) =>
                    {

                        var drawTarget = drawInstanceTargets[ node.DrawInstanceEntity ];
                        var i = drawTarget.InstanceIndex * node.NodeLength + node.NodeIndex;

                        var pInstance = offsetsOfDrawModel[ node.DrawModelEntity ].pVectorOffsetInBuffer;
                        pInstance[ i ] = new float4( pos.Value, 1.0f );

                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }
    }
}
