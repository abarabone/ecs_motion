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
                GroundResults = this.GetComponentDataFromEntity<GroundHitResultData>( isReadOnly: true ),
                Rotations = this.GetComponentDataFromEntity<Rotation>(),
                MotionCursors = this.GetComponentDataFromEntity<MotionCursorData>(),
                MotionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>(),
            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );

            return inputDeps;
        }


        struct AntWalkActionJob : IJobForEachWithEntity
            <AntWalkActionState, MoveHandlingData, CharacterLinkData>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

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
                ref AntWalkActionState state,
                [ReadOnly] ref MoveHandlingData hander,
                [ReadOnly] ref CharacterLinkData linker
            )
            {
                ref var acts = ref hander.ControlAction;

                var motion = new MotionOperator( this.Commands, this.MotionInfos, this.MotionCursors, linker.MainMotionEntity, jobIndex );
                
                motion.Start( Motion_ant.walking, isLooping: true, delayTime: 0.1f );

                this.Rotations[ linker.PostureEntity ] =
                    new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };
            }
        }

    }
}
