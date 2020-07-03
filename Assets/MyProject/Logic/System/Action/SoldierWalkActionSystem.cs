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

using Abarabone.Misc;
using Abarabone.Utilities;
using Abarabone.SystemGroup;
using Abarabone.Character;
using Abarabone.Motion;

namespace Abarabone.Character
{

    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    public class SoldierWalkActionSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;



        protected override void OnCreate()
        {
            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new SoldierWalkActionJob
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


        [BurstCompile]
        struct SoldierWalkActionJob : IJobForEachWithEntity
            <SoldierWalkActionState, MoveHandlingData, ObjectMainCharacterLinkData>
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
                Entity entity, int index,
                ref SoldierWalkActionState state,
                [ReadOnly] ref MoveHandlingData hander,
                [ReadOnly] ref ObjectMainCharacterLinkData linker
            )
            {
                ref var acts = ref hander.ControlAction;

                var motionInfo = this.MotionInfos[ linker.MotionEntity ];

                if( acts.IsChangeMotion )
                {
                    this.Commands.AddComponent( index, linker.MotionEntity,
                        new MotionInitializeData { MotionIndex = ( motionInfo.MotionIndex + 1 ) % 10, IsContinuous = true } );
                }

                if( !GroundResults[linker.PostureEntity].IsGround )
                {
                    if( motionInfo.MotionIndex != Motion_riku.jump02 )
                        this.Commands.AddComponent( index, linker.MotionEntity,
                            new MotionInitializeData { MotionIndex = Motion_riku.jump02, DelayTime = 0.1f, IsContinuous = true } );
                    return;
                }

                if( math.lengthsq(acts.MoveDirection) >= 0.01f )
                {
                    if( motionInfo.MotionIndex != Motion_riku.walk_stance )
                        this.Commands.AddComponent( index, linker.MotionEntity,
                            new MotionInitializeData { MotionIndex = Motion_riku.walk_stance, DelayTime = 0.1f, IsContinuous = true } );

                    this.Rotations[ linker.PostureEntity ] =
                        new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
                }
                else
                {
                    if( motionInfo.MotionIndex != Motion_riku.stand_stance )
                        this.Commands.AddComponent( index, linker.MotionEntity,
                            new MotionInitializeData { MotionIndex = Motion_riku.stand_stance, DelayTime = 0.2f, IsContinuous = true } );

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
