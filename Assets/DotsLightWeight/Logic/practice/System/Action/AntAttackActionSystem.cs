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
    using Abarabone.Arms;
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

            var triggers = this.GetComponentDataFromEntity<FunctionUnit.TriggerData>();


            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .WithReadOnly(motionInfos)
                .WithReadOnly(targetposs)
                .WithReadOnly(poss)
                .WithReadOnly(mvspds)
                .WithNativeDisableParallelForRestriction(motionCursors)
                .WithNativeDisableParallelForRestriction(triggers)// ヤバいかも
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref AntAction.AttackState state,
                        in ActionState.MotionLinkDate mlink,
                        in ActionState.PostureLinkData plink,
                        in TargetSensorHolderLink.HolderLinkData holderLink,
                        in AntAction.AttackTimeRange tr_,
                        in DynamicBuffer<FunctionHolder.LinkData> flinks
                    )
                =>
                    {
                        var tr = tr_;

                        switch (state.Phase)
                        {
                            case 0:
                                initPhase_(ref state, in mlink, in plink);
                                break;
                            case 1:
                                prevShotPhase_(ref state, in mlink);
                                break;
                            case 2:
                                shotPhase_(ref state, in mlink, flinks);
                                break;
                            case 3:
                                afterShotPhase_(ref state, in mlink, in plink, in holderLink);
                                break;
                        }

                        return;


                        void initPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink,
                            in ActionState.PostureLinkData plink)
                        {

                            cmd.AddComponent(entityInQueryIndex, plink.PostureEntity,
                                new Move.EasingSpeedData
                                {
                                    Rate = 3.0f,
                                    TargetSpeedPerSec = 0.0f,
                                }
                            );


                            var motion = new MotionOperator
                                (cmd, motionInfos, motionCursors, mlink.MotionEntity, entityInQueryIndex);

                            motion.Start(Motion_ant.attack02, isLooping: true, delayTime: 0.1f);
                            

                            state.Phase++;
                        }


                        void prevShotPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink)
                        {
                            var cursor = motionCursors[mlink.MotionEntity];

                            var normalPosision = cursor.CurrentPosition * math.rcp(cursor.TotalLength);

                            if (normalPosision >= tr.st)//0.2f)
                            {
                                state.Phase++;
                            }
                        }


                        void shotPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink,
                            in DynamicBuffer<FunctionHolder.LinkData> flinks)
                        {

                            var acidgun = flinks[0].FunctionEntity;

                            triggers[acidgun] = new FunctionUnit.TriggerData
                            {
                                IsTriggered = true,
                            };


                            var cursor = motionCursors[mlink.MotionEntity];

                            var normalPosision = cursor.CurrentPosition * math.rcp(cursor.TotalLength);

                            if (normalPosision >= tr.ed)//0.3f)
                            {
                                state.Phase++;
                            }
                        }


                        void afterShotPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink,
                            in ActionState.PostureLinkData plink,
                            in TargetSensorHolderLink.HolderLinkData holderLink)
                        {
                            var targetpos = targetposs[holderLink.HolderEntity].Position;
                            var originpos = poss[plink.PostureEntity].Value;

                            if (math.distancesq(targetpos, originpos) > 15.0f * 15.0f)
                            {
                                cmd.RemoveComponent<AntAction.AttackState>(entityInQueryIndex, entity);
                                cmd.AddComponent<AntAction.WalkState>(entityInQueryIndex, entity);

                                cmd.AddComponent(entityInQueryIndex, plink.PostureEntity,
                                    new Move.EasingSpeedData
                                    {
                                        Rate = 5.0f,
                                        TargetSpeedPerSec = mvspds[plink.PostureEntity].SpeedPerSecMax,
                                    }
                                );

                                return;
                            }


                            var cursor = motionCursors[mlink.MotionEntity];

                            var normalPosision = cursor.CurrentPosition * math.rcp(cursor.TotalLength);

                            if (normalPosision > 0.95f)
                            {
                                state.Phase = 0;
                            }

                        }
                    }
                )
                .ScheduleParallel();
                

        }

    }
}

