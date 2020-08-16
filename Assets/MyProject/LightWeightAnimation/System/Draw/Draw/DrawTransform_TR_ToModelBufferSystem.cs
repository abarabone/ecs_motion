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

using Abarabone.Authoring;
using Abarabone.CharacterMotion;
using Abarabone.SystemGroup;

namespace Abarabone.Draw
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    //[UpdateAfter(typeof())]
    [UpdateBefore( typeof( BeginDrawCsBarier ) )]
    public class DrawTransform_TR_ToModelBufferSystem : SystemBase//JobComponentSystem
    {

        BeginDrawCsBarier presentationBarier;// 次のフレームまでにジョブが完了することを保証

        protected override void OnStartRunning()
        {
            this.presentationBarier = this.World.GetExistingSystem<BeginDrawCsBarier>();
        }


        //protected unsafe override JobHandle OnUpdate( JobHandle inputDeps )
        protected unsafe override void OnUpdate()
        {

            this.Entities
                .WithBurst()
                .WithNone<NonUniformScale>()
                .ForEach(
                    (
                        in DrawTransform.VectorBufferData buffer,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {

                        if (buffer.pVectorPerBoneInBuffer == null) return;


                        var p = buffer.pVectorPerBoneInBuffer;
                        p[0] = new float4(pos.Value, 1.0f);
                        p[1] = rot.Value.value;

                    }
                )
                //.Schedule();
                .ScheduleParallel();


            this.presentationBarier.AddJobHandleForProducer( this.Dependency );
            //return inputDeps;
        }



    }

}
