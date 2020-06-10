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


using Abarabone.Arthuring;
using Abarabone.SystemGroup;
using Abarabone.Utilities;
using Abarabone.Geometry;
using Abarabone.Character;
using Abarabone.Motion;//

namespace Abarabone.Physics
{

    //[DisableAutoCreation]
    //[UpdateInGroup( typeof( BonePhysicsSystemGroup ) )]
    [UpdateBefore( typeof( MotionStreamProgressAndInterporationSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class PhysicsBoneInitializeSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;


        protected override void OnCreate()
        {
            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var commandBuffer = this.ecb.CreateCommandBuffer();


            inputDeps = new PhysicsBoneInitializeJob
            {
                Commands = commandBuffer.ToConcurrent(),
                Translations = this.GetComponentDataFromEntity<Translation>(),// isReadOnly: true ),
                Rotations = this.GetComponentDataFromEntity<Rotation>(),// isReadOnly: true ),

            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }


        struct PhysicsBoneInitializeJob : IJobForEachWithEntity
            <BoneInitializeData, /*Translation, Rotation,*/ PhysicsVelocity>
        {

            public EntityCommandBuffer.Concurrent Commands;

            [NativeDisableParallelForRestriction]
            //[ReadOnly]
            public ComponentDataFromEntity<Translation> Translations;
            [NativeDisableParallelForRestriction]
            //[ReadOnly]
            public ComponentDataFromEntity<Rotation> Rotations;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref BoneInitializeData init,
                //ref Translation pos,
                //ref Rotation rot,
                ref PhysicsVelocity v
            )
            {

                var basepos = this.Translations[ init.PostureEntity ];
                var baserot = this.Rotations[ init.PostureEntity ];

                var pos = this.Translations[ entity ];//
                var rot = this.Rotations[ entity ];//

                pos.Value = basepos.Value + math.mul(baserot.Value, pos.Value);
                rot.Value = math.mul(baserot.Value, rot.Value);
                v = new PhysicsVelocity { Linear = float3.zero, Angular = float3.zero };

                this.Translations[ entity ] = pos;//
                this.Rotations[ entity ] = rot;//

                Commands.RemoveComponent<BoneInitializeData>( index, entity );
            }
        }
    }
}
