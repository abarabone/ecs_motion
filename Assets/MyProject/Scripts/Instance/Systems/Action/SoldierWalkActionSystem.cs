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
    [UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    public class SoldierWalkActionSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;



        protected override void OnCreate()
        {
            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new SolderWalkActionJob
            {
                Commands = this.ecb.CreateCommandBuffer().ToConcurrent(),
                MotionInfos = this.GetComponentDataFromEntity<MotionInfoData>( isReadOnly: true ),
                GroundResults = this.GetComponentDataFromEntity<GroundHitResultData>( isReadOnly: true ),
                Rotations = this.GetComponentDataFromEntity<Rotation>(),
            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );

            return inputDeps;
        }


        struct SolderWalkActionJob : IJobForEachWithEntity
            <WalkActionState, MoveHandlingData, CharacterLinkData>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [ReadOnly] public ComponentDataFromEntity<MotionInfoData> MotionInfos;
            [ReadOnly] public ComponentDataFromEntity<GroundHitResultData> GroundResults;
            [NativeDisableParallelForRestriction]
            [WriteOnly] public ComponentDataFromEntity<Rotation> Rotations;


            public void Execute(
                Entity entity, int index,
                ref WalkActionState state,
                [ReadOnly] ref MoveHandlingData hander,
                [ReadOnly] ref CharacterLinkData linker
            )
            {
                ref var acts = ref hander.ControlAction;

                var motionInfo = this.MotionInfos[ linker.MotionEntity ];

                if( acts.IsChangeMotion )
                {
                    this.Commands.AddComponent( index, linker.MotionEntity,
                        new MotionInitializeData { MotionIndex = ( motionInfo.MotionIndex + 1 ) % 10 } );
                }

                if( !GroundResults[linker.PostureEntity].IsGround )
                {
                    if( motionInfo.MotionIndex != 0 )
                        this.Commands.AddComponent( index, linker.MotionEntity,
                            new MotionInitializeData { MotionIndex = 0 } );
                    return;
                }

                if( math.lengthsq(acts.MoveDirection) > 0.0f )
                {
                    if( motionInfo.MotionIndex != 1 )
                        this.Commands.AddComponent( index, linker.MotionEntity,
                            new MotionInitializeData { MotionIndex = 1 } );

                    this.Rotations[ linker.PostureEntity ] =
                        new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
                }
                else
                {
                    if( motionInfo.MotionIndex != 9 )
                        this.Commands.AddComponent( index, linker.MotionEntity,
                            new MotionInitializeData { MotionIndex = 9 } );

                    this.Rotations[ linker.PostureEntity ] =
                        new Rotation { Value = acts.HorizontalRotation };
                }
                
            }
        }

    }
}
