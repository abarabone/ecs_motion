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
    public class AntrWalkActionSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;



        protected override void OnCreate()
        {
            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new AntWalkActionJob
            {
                Commands = this.ecb.CreateCommandBuffer().ToConcurrent(),

                MotionInfos = this.GetComponentDataFromEntity<MotionInfoData>( isReadOnly: true ),
                MotionCursors = this.GetComponentDataFromEntity<MotionCursorData>(),
                MotionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>(),
                
                WallHitResults = this.GetComponentDataFromEntity<WallHitResultData>( isReadOnly: true ),
                Wallings = this.GetComponentDataFromEntity<WallHunggingData>( isReadOnly: true ),
                
                Rotations = this.GetComponentDataFromEntity<Rotation>(),
            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );

            return inputDeps;
        }


        [RequireComponentTag(typeof(AntTag))]
        struct AntWalkActionJob : IJobForEachWithEntity
            <AntWalkActionState, MoveHandlingData, CharacterLinkData>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [ReadOnly] public ComponentDataFromEntity<MotionInfoData> MotionInfos;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<MotionCursorData> MotionCursors;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<MotionBlend2WeightData> MotionWeights;
            
            [ReadOnly] public ComponentDataFromEntity<WallHitResultData> WallHitResults;
            [ReadOnly] public ComponentDataFromEntity<WallHunggingData> Wallings;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public ComponentDataFromEntity<Rotation> Rotations;



            public void Execute(
                Entity entity, int jobIndex,
                ref AntWalkActionState state,
                [ReadOnly] ref MoveHandlingData hander,
                [ReadOnly] ref CharacterLinkData linker
            )
            {
                ref var acts = ref hander.ControlAction;

                var motion = new MotionOperator( this.Commands, this.MotionInfos, this.MotionCursors, linker.MainMotionEntity, jobIndex );

                motion.Start( Motion_ant.walking, isLooping: true, delayTime: 0.1f );

                //this.Rotations[ linker.PostureEntity ] =
                //    new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
                

                if( this.Wallings.Exists(linker.PostureEntity) )
                {
                    if( this.Wallings[linker.PostureEntity].State >= 2 )
                    {
                        this.Commands.RemoveComponent<WallHunggingData>( jobIndex, linker.PostureEntity );

                        this.Commands.AddComponent( jobIndex, linker.PostureEntity, new PhysicsVelocity { } );
                        this.Commands.AddComponent( jobIndex, linker.PostureEntity, new WallHitResultData { } );
                    }
                }
                else
                {
                    if( this.WallHitResults[ linker.PostureEntity ].IsHit )
                    {
                        this.Commands.RemoveComponent<PhysicsVelocity>( jobIndex, linker.PostureEntity );
                        this.Commands.RemoveComponent<WallHitResultData>( jobIndex, linker.PostureEntity );

                        this.Commands.AddComponent( jobIndex, linker.PostureEntity, new WallHunggingData { } );
                    }
                }

            }
        }

    }
}
