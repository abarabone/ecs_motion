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
using Abss.Instance;
using Abss.Motion;

namespace Abss.Character
{

    //[DisableAutoCreation]
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
            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );

            return inputDeps;
        }


        struct SolderWalkActionJob : IJobForEachWithEntity
            <WalkActionStateData, CharacterLinkData>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [ReadOnly] public ComponentDataFromEntity<MotionInfoData> MotionInfos;


            public void Execute(
                Entity entity, int index,
                ref WalkActionStateData state,
                [ReadOnly] ref CharacterLinkData linker
            )
            {
                //if( this.Acts.IsChangeMotion )
                //{
                //    var motionInfo = this.MotionInfos[ linker.MotionEntity ];
                //    this.Commands.AddComponent( index, linker.MotionEntity, new MotionInitializeData { MotionIndex = ( motionInfo.MotionIndex + 1 ) % 10 } );
                //}
            }
        }

    }
}
