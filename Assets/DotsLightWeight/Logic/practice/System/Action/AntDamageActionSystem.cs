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
    public class AntrDamageActionSystem : DependencyAccessableSystemBase
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


            var currentTime = this.Time.ElapsedTime;

            this.Entities
                .WithBurst()
                //.WithAll<AntTag>()
                .WithReadOnly(motionInfos)
                //.WithReadOnly(targetposs)
                //.WithReadOnly(poss)
                //.WithReadOnly(mvspds)
                .WithNativeDisableParallelForRestriction(motionCursors)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref AntAction.DamageState state,
                        in ActionState.MotionLinkDate mlink,
                        in ActionState.PostureLinkData plink,
                        in TargetSensorHolderLink.HolderLinkData holderLink
                    )
                =>
                    {
                        switch (state.Phase)
                        {
                            case 0:
                                initPhase_(ref state, in mlink, in plink);
                                break;
                            case 1:
                                break;
                            case 2:
                                break;
                            case 3:
                                break;
                        }


                        if (currentTime <= state.EntTime) return;

                        cmd.RemoveComponent<AntAction.DamageState>(entityInQueryIndex, entity);

                        return;


                        void initPhase_(
                            ref AntAction.DamageState state,
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

                            motion.Start(Motion_ant.seaching, isLooping: true, delayTime: 0.1f, scale: 3.0f);

                            state.Phase++;
                        }

                    }
                )
                .ScheduleParallel();
                

        }

    }
}

