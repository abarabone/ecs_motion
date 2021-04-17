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
    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
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


            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .WithReadOnly(motionInfos)
                .WithReadOnly(targetposs)
                .WithReadOnly(poss)
                .WithNativeDisableParallelForRestriction(motionCursors)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref AntAction.AttackState state,
                        //in MoveHandlingData hander,
                        in ObjectMainCharacterLinkData link,
                        in TargetSensorHolderLink.HolderLinkData holderLink
                    )
                =>
                    {

                        if (state.Phase == 0)
                        {
                            cmd.RemoveComponent<AntAction.AttackState>(entityInQueryIndex, link.PostureEntity);
                            state.Phase++;
                        }



                        var motion = new MotionOperator
                            (cmd, motionInfos, motionCursors, link.MotionEntity, entityInQueryIndex);

                        motion.Start(Motion_ant.attack01, isLooping: true, delayTime: 0.1f);



                        var targetpos = targetposs[holderLink.HolderEntity].Position;
                        var originpos = poss[link.PostureEntity].Value;

                        if (math.distancesq(targetpos, originpos) > 1.0f)
                        {
                            cmd.RemoveComponent<AntAction.AttackState>(entityInQueryIndex, entity);
                            cmd.AddComponent<AntAction.WalkState>(entityInQueryIndex, entity);
                        }
                    }
                )
                .ScheduleParallel();
        }

    }
}

