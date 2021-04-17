//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Physics;
//using Unity.Physics.Systems;

//using Collider = Unity.Physics.Collider;
//using SphereCollider = Unity.Physics.SphereCollider;


//namespace Abarabone.Character
//{

//    using Abarabone.Misc;
//    using Abarabone.Utilities;
//    using Abarabone.SystemGroup;
//    using Abarabone.Character;
//    using Abarabone.CharacterMotion;
//    using Motion = Abarabone.CharacterMotion.Motion;
//    using Abarabone.Dependency;


//    /// <summary>
//    /// 歩き時のアクションステート
//    /// 
//    /// </summary>
//    //[DisableAutoCreation]
//    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
//    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
//    public class SoldierWalkActionSystem : DependencyAccessableSystemBase
//    {


//        CommandBufferDependency.Sender cmddep;


//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
//        }

//        protected override void OnUpdate()
//        {
//            using var cmdScope = this.cmddep.WithDependencyScope();


//            //inputDeps = new SoldierWalkActionJob
//            //{
//            //    Commands = this.ecb.CreateCommandBuffer().AsParallelWriter(),
//            //    MotionInfos = this.GetComponentDataFromEntity<Motion.InfoData>( isReadOnly: true ),
//            //    GroundResults = this.GetComponentDataFromEntity<GroundHitResultData>( isReadOnly: true ),
//            //    Rotations = this.GetComponentDataFromEntity<Rotation>(),
//            //    MotionCursors = this.GetComponentDataFromEntity<Motion.CursorData>(),
//            //    MotionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>(),
//            //}
//            //.Schedule( this, inputDeps );
//            //this.ecb.AddJobHandleForProducer( inputDeps );

//            var commands = this.cmddep.CreateCommandBuffer().AsParallelWriter();
//            var motionInfos = this.GetComponentDataFromEntity<Motion.InfoData>(isReadOnly: true);
//            var groundResults = this.GetComponentDataFromEntity<GroundHitResultData>(isReadOnly: true);
//            var rotations = this.GetComponentDataFromEntity<Rotation>();
//            //var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>();
//            //var motionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>();

//            this.Entities
//                .WithBurst()
//                .WithReadOnly(motionInfos)
//                .WithReadOnly(groundResults)
//                .WithNativeDisableParallelForRestriction(rotations)
//                //.WithNativeDisableParallelForRestriction(motionCursors)
//                //.WithNativeDisableParallelForRestriction(motionWeights)
//                .ForEach(
//                    (
//                        Entity entity, int entityInQueryIndex,
//                        ref SoldierWalkActionState state,
//                        in MoveHandlingData hander,
//                        in ObjectMainCharacterLinkData linker
//                    )
//                =>
//                    {
//                        //ref var acts = ref hander.ControlAction;
//                        var acts = hander.ControlAction;

//                        var motionInfo = motionInfos[linker.MotionEntity];

//                        if (acts.IsChangeMotion)
//                        {
//                            commands.AddComponent(entityInQueryIndex, linker.MotionEntity,
//                                new Motion.InitializeData
//                                {
//                                    MotionIndex = (motionInfo.MotionIndex + 1) % 10,
//                                    IsContinuous = true,
//                                }
//                            );
//                        }

//                        if (!groundResults[linker.PostureEntity].IsGround)
//                        {
//                            if (motionInfo.MotionIndex != Motion_riku.jump02)
//                                commands.AddComponent(entityInQueryIndex, linker.MotionEntity,
//                                    new Motion.InitializeData
//                                    {
//                                        MotionIndex = Motion_riku.jump02,
//                                        DelayTime = 0.1f,
//                                        IsContinuous = true,
//                                    }
//                                );
//                            return;
//                        }

//                        if (math.lengthsq(acts.MoveDirection) >= 0.01f)
//                        {
//                            if (motionInfo.MotionIndex != Motion_riku.walk_stance)
//                            {
//                                commands.AddComponent(entityInQueryIndex, linker.MotionEntity,
//                                    new Motion.InitializeData
//                                    {
//                                        MotionIndex = Motion_riku.walk_stance,
//                                        DelayTime = 0.1f,
//                                        IsContinuous = true,
//                                    }
//                                );
//                            }

//                            rotations[linker.PostureEntity] =
//                                new Rotation
//                                {
//                                    Value = quaternion.LookRotation(math.normalize(acts.MoveDirection), math.up()),
//                                };
//                        }
//                        else
//                        {
//                            if (motionInfo.MotionIndex != Motion_riku.stand_stance)
//                            {
//                                commands.AddComponent(entityInQueryIndex, linker.MotionEntity,
//                                    new Motion.InitializeData
//                                    {
//                                        MotionIndex = Motion_riku.stand_stance,
//                                        DelayTime = 0.2f,
//                                        IsContinuous = true,
//                                    }
//                                );
//                            }
//                            //rotations[ linker.PostureEntity ] =
//                            //    new Rotation { Value = acts.HorizontalRotation };
//                        }

//                        //var m = this.MotionWeights[ linker.MainMotionEntity ];
//                        //var l = math.length( acts.MoveDirection );
//                        //m.SetWeight( 1.0f - l, l );
//                        //this.MotionWeights[ linker.MainMotionEntity ] = m;

//                        //var motionCursor = this.MotionCursors[ linker.MotionEntity ];

//                        //motionCursor.Timer.TimeProgress = motionCursor.Timer.TimeLength * 0.5f;
//                        //motionCursor.Timer.TimeScale = 0.5f;

//                        //this.MotionCursors[ linker.MotionEntity ] = motionCursor;
//                    }
//                )
//                .ScheduleParallel();
//        }


//        //[BurstCompile]
//        //struct SoldierWalkActionJob : IJobForEachWithEntity
//        //    <SoldierWalkActionState, MoveHandlingData, ObjectMainCharacterLinkData>
//        //{

//        //    public EntityCommandBuffer.ParallelWriter Commands;

//        //    [ReadOnly] public ComponentDataFromEntity<Motion.InfoData> MotionInfos;
//        //    [ReadOnly] public ComponentDataFromEntity<GroundHitResultData> GroundResults;

//        //    [NativeDisableParallelForRestriction]
//        //    [WriteOnly] public ComponentDataFromEntity<Rotation> Rotations;

//        //    [NativeDisableParallelForRestriction]
//        //    public ComponentDataFromEntity<Motion.CursorData> MotionCursors;
//        //    [NativeDisableParallelForRestriction]
//        //    public ComponentDataFromEntity<MotionBlend2WeightData> MotionWeights;


//        //    public void Execute(
//        //        Entity entity, int index,
//        //        ref SoldierWalkActionState state,
//        //        [ReadOnly] ref MoveHandlingData hander,
//        //        [ReadOnly] ref ObjectMainCharacterLinkData linker
//        //    )
//        //    {
//        //        ref var acts = ref hander.ControlAction;

//        //        var motionInfo = this.MotionInfos[ linker.MotionEntity ];

//        //        if( acts.IsChangeMotion )
//        //        {
//        //            this.Commands.AddComponent( index, linker.MotionEntity,
//        //                new Motion.InitializeData { MotionIndex = ( motionInfo.MotionIndex + 1 ) % 10, IsContinuous = true } );
//        //        }

//        //        if( !GroundResults[linker.PostureEntity].IsGround )
//        //        {
//        //            if( motionInfo.MotionIndex != Motion_riku.jump02 )
//        //                this.Commands.AddComponent( index, linker.MotionEntity,
//        //                    new Motion.InitializeData { MotionIndex = Motion_riku.jump02, DelayTime = 0.1f, IsContinuous = true } );
//        //            return;
//        //        }

//        //        if( math.lengthsq(acts.MoveDirection) >= 0.01f )
//        //        {
//        //            if( motionInfo.MotionIndex != Motion_riku.walk_stance )
//        //                this.Commands.AddComponent( index, linker.MotionEntity,
//        //                    new Motion.InitializeData { MotionIndex = Motion_riku.walk_stance, DelayTime = 0.1f, IsContinuous = true } );

//        //            this.Rotations[ linker.PostureEntity ] =
//        //                new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
//        //        }
//        //        else
//        //        {
//        //            if( motionInfo.MotionIndex != Motion_riku.stand_stance )
//        //                this.Commands.AddComponent( index, linker.MotionEntity,
//        //                    new Motion.InitializeData { MotionIndex = Motion_riku.stand_stance, DelayTime = 0.2f, IsContinuous = true } );

//        //            //this.Rotations[ linker.PostureEntity ] =
//        //            //    new Rotation { Value = acts.HorizontalRotation };
//        //        }

//        //        //var m = this.MotionWeights[ linker.MainMotionEntity ];
//        //        //var l = math.length( acts.MoveDirection );
//        //        //m.SetWeight( 1.0f - l, l );
//        //        //this.MotionWeights[ linker.MainMotionEntity ] = m;

//        //        //var motionCursor = this.MotionCursors[ linker.MotionEntity ];

//        //        //motionCursor.Timer.TimeProgress = motionCursor.Timer.TimeLength * 0.5f;
//        //        //motionCursor.Timer.TimeScale = 0.5f;

//        //        //this.MotionCursors[ linker.MotionEntity ] = motionCursor;

//        //    }
//        //}

//    }
//}
