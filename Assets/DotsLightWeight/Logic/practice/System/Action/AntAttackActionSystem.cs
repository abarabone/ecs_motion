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
using Unity.Physics;
using Unity.Physics.Systems;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Abarabone.Character.Action
{
    using Abarabone.Dependency;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;
    using Abarabone.Targeting;
    using Motion = Abarabone.CharacterMotion.Motion;


    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(AntMoveDirectionSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class AntrAttackActionSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            var motionInfos = this.GetComponentDataFromEntity<Motion.InfoData>(isReadOnly: true);
            var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>();
            //var motionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>();

            var targetposs = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var mvspds = this.GetComponentDataFromEntity<Move.SpeedParamaterData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .WithReadOnly(motionInfos)
                .WithReadOnly(targetposs)
                .WithReadOnly(poss)
                .WithReadOnly(mvspds)
                .WithNativeDisableParallelForRestriction(motionCursors)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref AntAction.AttackState state,
                        in ActionState.MotionLinkDate mlink,
                        in ActionState.PostureLinkData plink,
                        in TargetSensorHolderLink.HolderLinkData holderLink
                    )
                =>
                    {

                        if (state.Phase == 0)
                        {
                            cmd.AddComponent(entityInQueryIndex, plink.PostureEntity, new Move.EasingSpeedData
                            {
                                Rate = 3.0f,
                                TargetSpeedPerSec = 0.0f,
                            });
                            state.Phase++;
                        }



                        var motion = new MotionOperator
                            (cmd, motionInfos, motionCursors, mlink.MotionEntity, entityInQueryIndex);

                        motion.Start(Motion_ant.attack02, isLooping: true, delayTime: 0.1f);



                        var targetpos = targetposs[holderLink.HolderEntity].Position;
                        var originpos = poss[plink.PostureEntity].Value;

                        if (math.distancesq(targetpos, originpos) > 15.0f * 15.0f)
                        {
                            cmd.RemoveComponent<AntAction.AttackState>(entityInQueryIndex, entity);
                            cmd.AddComponent<AntAction.WalkState>(entityInQueryIndex, entity);

                            cmd.AddComponent(entityInQueryIndex, plink.PostureEntity, new Move.EasingSpeedData
                            {
                                Rate = 5.0f,
                                TargetSpeedPerSec = mvspds[plink.PostureEntity].SpeedPerSecMax,
                            });
                        }
                    }
                )
                .ScheduleParallel();
        }

    }
}

