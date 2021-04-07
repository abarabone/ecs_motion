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
    using Motion = Abarabone.CharacterMotion.Motion;


    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    public class AntrWalkActionSystem : CommandSystemBase<BeginInitializationEntityCommandBufferSystem>
    {


        protected override void OnUpdateWith(EntityCommandBuffer commandbuffer)
        {
            var commands = commandbuffer.AsParallelWriter();

            var motionInfos = this.GetComponentDataFromEntity<Motion.InfoData>(isReadOnly: true);
            var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>();
            //var motionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>();

            //var wallHitResults = this.GetComponentDataFromEntity<WallHitResultData>(isReadOnly: true);
            //var wallings = this.GetComponentDataFromEntity<WallHunggingData>(isReadOnly: true);

            //var rotations = this.GetComponentDataFromEntity<Rotation>();
            //var gravityFactors = this.GetComponentDataFromEntity<PhysicsGravityFactor>();

            this.Entities
                .WithBurst()
                .WithAll<AntTag>()
                .WithReadOnly(motionInfos)
                .WithNativeDisableParallelForRestriction(motionCursors)
                //.WithNativeDisableParallelForRestriction(motionWeights)
                //.WithReadOnly(wallHitResults)
                //.WithReadOnly(wallings)
                //.WithNativeDisableParallelForRestriction(rotations)
                //.WithNativeDisableParallelForRestriction(gravityFactors)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref AntWalkActionState state,
                        in MoveHandlingData hander,
                        in ObjectMainCharacterLinkData linker
                    )
                =>
                    {
                        var acts = hander.ControlAction;

                        var motion = new MotionOperator(commands, motionInfos, motionCursors, linker.MotionEntity, entityInQueryIndex);

                        motion.Start(Motion_ant.walking, isLooping: true, delayTime: 0.1f);

                        //rotations[ linker.PostureEntity ] =
                        //    new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
                    }
                )
                .ScheduleParallel();
        }

    }
}
