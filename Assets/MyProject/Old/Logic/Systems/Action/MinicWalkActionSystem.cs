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

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;
using Abss.Character;
using Abss.Motion;

namespace Abss.Character
{

    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    public class MinicrWalkActionSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;



        protected override void OnCreate()
        {
            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new MinicWalkActionJob
            {
                Commands = this.ecb.CreateCommandBuffer().ToConcurrent(),
                MotionInfos = this.GetComponentDataFromEntity<MotionInfoData>( isReadOnly: true ),
                GroundResults = this.GetComponentDataFromEntity<GroundHitResultData>( isReadOnly: true ),
                Rotations = this.GetComponentDataFromEntity<Rotation>(),
                MotionCursors = this.GetComponentDataFromEntity<MotionCursorData>(),
                MotionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>(),
            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );

            return inputDeps;
        }


        struct MinicWalkActionJob : IJobForEachWithEntity
            <MinicWalkActionState, MoveHandlingData, CharacterLinkData>
        {

            public EntityCommandBuffer.Concurrent Commands;

            [ReadOnly] public ComponentDataFromEntity<MotionInfoData> MotionInfos;
            [ReadOnly] public ComponentDataFromEntity<GroundHitResultData> GroundResults;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public ComponentDataFromEntity<Rotation> Rotations;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<MotionCursorData> MotionCursors;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<MotionBlend2WeightData> MotionWeights;


            public void Execute(
                Entity entity, int jobIndex,
                ref MinicWalkActionState state,
                [ReadOnly] ref MoveHandlingData hander,
                [ReadOnly] ref CharacterLinkData linker
            )
            {
                ref var acts = ref hander.ControlAction;

                var motionInfo = this.MotionInfos[ linker.MainMotionEntity ];

                var motion = new MotionOperator( this.Commands, this.MotionInfos, this.MotionCursors, linker.MainMotionEntity, jobIndex );


                if( state.Phase == 1 )
                {
                    var cursor = this.MotionCursors[ linker.MainMotionEntity ];
                    if( cursor.CurrentPosition > cursor.TotalLength )
                        state.Phase = 0;

                    return;
                }


                if( acts.IsChangeMotion )
                {
                    MotionOp.Start( jobIndex, ref this.Commands, linker.MainMotionEntity, motionInfo, Motion_minic.slash01HL, true, 0.1f );
                    state.Phase = 1;
                    return;
                }

                if( !GroundResults[linker.PostureEntity].IsGround )
                {
                    if( motionInfo.MotionIndex != (int)Motion_minic.jumpdown )
                        this.Commands.AddComponent( jobIndex, linker.MainMotionEntity,
                            new MotionInitializeData { MotionIndex = 0, DelayTime = 0.1f, IsContinuous = true } );
                    return;
                }

                if( math.lengthsq( acts.MoveDirection ) >= 0.5f*0.5f )
                {
                    motion.Start( Motion_minic.run01, isLooping: true, delayTime: 0.05f );

                    this.Rotations[ linker.PostureEntity ] =
                        new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
                }
                else if( math.lengthsq(acts.MoveDirection) >= 0.01f )
                {
                    if( motionInfo.MotionIndex != (int)Motion_minic.walk02 )
                        this.Commands.AddComponent( jobIndex, linker.MainMotionEntity,
                            new MotionInitializeData { MotionIndex = (int)Motion_minic.walk02, DelayTime = 0.1f, IsContinuous = true } );

                    this.Rotations[ linker.PostureEntity ] =
                        new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
                }
                else
                {
                    if( motionInfo.MotionIndex != (int)Motion_minic.stand02 )
                        this.Commands.AddComponent( jobIndex, linker.MainMotionEntity,
                            new MotionInitializeData { MotionIndex = (int)Motion_minic.stand02, DelayTime = 0.2f, IsContinuous = true } );

                    //this.Rotations[ linker.PostureEntity ] =
                    //    new Rotation { Value = acts.HorizontalRotation };
                }

                //var m = this.MotionWeights[ linker.MainMotionEntity ];
                //var l = math.length( acts.MoveDirection );
                //m.SetWeight( 1.0f - l, l );
                //this.MotionWeights[ linker.MainMotionEntity ] = m;

                //var motionCursor = this.MotionCursors[ linker.MotionEntity ];

                //motionCursor.Timer.TimeProgress = motionCursor.Timer.TimeLength * 0.5f;
                //motionCursor.Timer.TimeScale = 0.5f;

                //this.MotionCursors[ linker.MotionEntity ] = motionCursor;

            }
        }

    }
}
