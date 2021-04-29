using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace DotsLite.Draw
{
    
    using DotsLite.CharacterMotion;
    using DotsLite.SystemGroup;


    ////[UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    ////[UpdateAfter( typeof( DrawCullingSystem ) )]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.Marking))]
    public class MarkDrawTargetMotionSystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var targets = this.GetComponentDataFromEntity<DrawInstance.TargetWorkData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(targets)
                .ForEach(
                    (
                        ref Motion.DrawCullingData curring
                    ) =>
                    {

                        curring.IsDrawTarget = targets[curring.DrawInstanceEntity].DrawInstanceId != -1;

                    }
                )
                .ScheduleParallel();

        }


    }

}
