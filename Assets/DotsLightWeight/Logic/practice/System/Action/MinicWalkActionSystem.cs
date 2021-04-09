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
    using Abarabone.Common;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;
    using Abarabone.Dependency;
    using Motion = Abarabone.CharacterMotion.Motion;


    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    public class MinicrWalkActionSystem : DependencyAccessableSystemBase
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


            var commands = this.cmddep.CreateCommandBuffer().AsParallelWriter();

            var motionInfos = this.GetComponentDataFromEntity<Motion.InfoData>(isReadOnly: true);
            var groundResults = this.GetComponentDataFromEntity<GroundHitResultData>(isReadOnly: true);
            var rotations = this.GetComponentDataFromEntity<Rotation>();
            var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>();
            var motionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>();

            this.Entities
                .WithReadOnly(motionInfos)
                .WithReadOnly(groundResults)
                .WithNativeDisableParallelForRestriction(rotations)
                .WithNativeDisableParallelForRestriction(motionCursors)
                //.WithNativeDisableParallelForRestriction(motionWeights)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref MinicWalkActionState state,
                        in MoveHandlingData hander,
                        in ObjectMainCharacterLinkData linker
                    )
                =>
                    {
                        var acts = hander.ControlAction;

                        var motionInfo = motionInfos[linker.MotionEntity];

                        var motion = new MotionOperator(commands, motionInfos, motionCursors, linker.MotionEntity, entityInQueryIndex);


                        if (state.Phase == 1)
                        {
                            var cursor = motionCursors[linker.MotionEntity];
                            if (cursor.CurrentPosition > cursor.TotalLength)
                                state.Phase = 0;

                            return;
                        }


                        if (acts.IsChangeMotion)
                        {
                            MotionOp.Start(entityInQueryIndex, ref commands, linker.MotionEntity, motionInfo, Motion_minic.slash01VU, false, 0.1f);
                            state.Phase = 1;
                            return;
                        }

                        if (!groundResults[linker.PostureEntity].IsGround)
                        {
                            if (motionInfo.MotionIndex != (int)Motion_minic.jumpdown)
                                commands.AddComponent(entityInQueryIndex, linker.MotionEntity,
                                    new Motion.InitializeData
                                    {
                                        MotionIndex = 0,
                                        DelayTime = 0.1f,
                                        IsContinuous = true,
                                    }
                                );
                            return;
                        }

                        switch (math.lengthsq(acts.MoveDirection))
                        {
                            case var distsq when distsq >= 0.5f * 0.5f:
                                {
                                    motion.Start(Motion_minic.run01, isLooping: true, delayTime: 0.05f);

                                    rotations[linker.PostureEntity] =
                                        new Rotation
                                        {
                                            Value = quaternion.LookRotation(math.normalize(acts.MoveDirection), math.up()),
                                        };
                                } break;
                            case var distsq when distsq >= 0.01f:
                                {
                                    if (motionInfo.MotionIndex != (int)Motion_minic.walk02)
                                    {
                                        commands.AddComponent(entityInQueryIndex, linker.MotionEntity,
                                            new Motion.InitializeData
                                            {
                                                MotionIndex = (int)Motion_minic.walk02,
                                                DelayTime = 0.1f,
                                                IsContinuous = true,
                                            }
                                        );
                                    }
                                    rotations[linker.PostureEntity] =
                                        new Rotation
                                        {
                                            Value = quaternion.LookRotation(math.normalize(acts.MoveDirection), math.up()),
                                        };
                                } break;
                            default:
                                {
                                    if (motionInfo.MotionIndex != (int)Motion_minic.stand02)
                                    {
                                        commands.AddComponent(entityInQueryIndex, linker.MotionEntity,
                                            new Motion.InitializeData
                                            {
                                                MotionIndex = (int)Motion_minic.stand02,
                                                DelayTime = 0.2f,
                                                IsContinuous = true,
                                            }
                                        );

                                    }
                                    //rotations[ linker.PostureEntity ] =
                                    //    new Rotation { Value = acts.HorizontalRotation };
                                }
                                break;
                        }
                        //var m = this.MotionWeights[ linker.MainMotionEntity ];
                        //var l = math.length( acts.MoveDirection );
                        //m.SetWeight( 1.0f - l, l );
                        //this.MotionWeights[ linker.MainMotionEntity ] = m;

                        //var motionCursor = motionCursors[ linker.MotionEntity ];

                        //motionCursor.Timer.TimeProgress = motionCursor.Timer.TimeLength * 0.5f;
                        //motionCursor.Timer.TimeScale = 0.5f;

                        //motionCursors[ linker.MotionEntity ] = motionCursor;
                    }
                )
                .ScheduleParallel();
        }


    }
}
