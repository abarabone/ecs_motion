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

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    //[UpdateBefore(typeof( BeginDrawCsBarier ) )]
    public class PsylliumToDrawInstanceSystem : JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        protected override unsafe JobHandle OnUpdate( JobHandle inputDeps )
        {

            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModelBoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModelInstanceOffsetData>( isReadOnly: true );

            inputDeps = this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .ForEach(
                    (
                        in DrawInstanceTargetWorkData target,
                        in DrawIndexOfModelData linker,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {

                        var i = target.InstanceIndex * 2;

                        var pInstance = offsetsOfDrawModel[ linker.ModelEntity ].pVectorOffsetInBuffer;
                        pInstance[ i + 0 ] = new float4( pos.Value, 1.0f );
                        pInstance[ i + 1 ] = math.forward( rot.Value ).As_float4();

                    }
                )
                .Schedule( inputDeps );


            this.presentationBarier.AddJobHandleForProducer( inputDeps );
            return inputDeps;
        }
    }
}
