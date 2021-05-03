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

namespace DotsLite.Character.Action
{
    using DotsLite.Dependency;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Targeting;
    using DotsLite.Arms;
    using Motion = DotsLite.CharacterMotion.Motion;


    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(AntMoveDirectionSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class DeadActionSystem : DependencyAccessableSystemBase
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


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var motionInfos = this.GetComponentDataFromEntity<Motion.InfoData>(isReadOnly: true);
            var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>();
            //var motionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>();

            var targetposs = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var mvspds = this.GetComponentDataFromEntity<Move.SpeedParamaterData>(isReadOnly: true);


            var currentTime = this.Time.ElapsedTime;

            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .WithAll<CharacterAction.DamageState>()
                .WithReadOnly(motionInfos)
                .WithNativeDisableParallelForRestriction(motionCursors)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref CharacterAction.DeadState state,
                        in ActionState.MotionLinkDate mlink,
                        in ActionState.PostureLinkData plink,
                        in ActionState.BinderLinkData blink
                    )
                =>
                    {
                        var eqi = entityInQueryIndex;

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


                        if (currentTime <= state.RemoveTime) return;

                        cmd.DestroyEntity(eqi, blink.BinderEntity);

                        return;


                        void initPhase_(
                            ref CharacterAction.DeadState state,
                            in ActionState.MotionLinkDate mlink,
                            in ActionState.PostureLinkData plink)
                        {

                            //cmd.AddComponent(eqi, plink.PostureEntity,
                            //    new Move.EasingSpeedData
                            //    {
                            //        Rate = 3.0f,
                            //        TargetSpeedPerSec = 0.0f,
                            //    }
                            //);
                            cmd.RemoveComponent<GroundHitWallingData>(eqi, plink.PostureEntity);


                            var motion = new MotionOperator
                                (cmd, motionInfos, motionCursors, mlink.MotionEntity, eqi);

                            motion.Start(Motion_ant.dead, isLooping: true, delayTime: 0.1f, scale: 0.2f);

                            state.Phase++;
                        }

                    }
                )
                .ScheduleParallel();


        }

    }
}

