using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace Abarabone.Draw
{
    using Abarabone.Authoring;
    using Abarabone.CharacterMotion;
    using Abarabone.SystemGroup;


    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    [UpdateAfter( typeof( DrawCullingSystem ) )]
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
                        ref Motion.DrawCurringData curring
                    ) =>
                    {

                        curring.IsDrawTarget = targets[curring.DrawInstanceEntity].DrawInstanceId != -1;

                    }
                )
                .ScheduleParallel();

        }


    }

}
