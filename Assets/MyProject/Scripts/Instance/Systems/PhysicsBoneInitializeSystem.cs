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

using Abss.Cs;
using Abss.Arthuring;
using Abss.SystemGroup;
using Abss.Utilities;
using Abss.Geometry;
using Abss.Instance;
using Abss.Motion;//

namespace Abss.Physics
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( BonePhysicsSystemGroup ) )]
    public class PhysicsBoneInitializeSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;


        protected override void OnCreate()
        {
            this.ecb = World.Active.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            var commandBuffer = this.ecb.CreateCommandBuffer();


            inputDeps = new PhysicsBoneInitializeJob
            {
                Commands = commandBuffer.ToConcurrent(),
                Translations = this.GetComponentDataFromEntity<Translation>( isReadOnly: true ),
                Rotations = this.GetComponentDataFromEntity<Rotation>( isReadOnly: true ),

            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );


            return inputDeps;
        }


        struct PhysicsBoneInitializeJob : IJobForEachWithEntity
            <BoneInitializeData, Translation, Rotation>
        {

            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;

            [ReadOnly]
            public ComponentDataFromEntity<Translation> Translations;
            [ReadOnly]
            public ComponentDataFromEntity<Rotation> Rotations;


            public void Execute(
                Entity entity, int index,
                [ReadOnly] ref BoneInitializeData init,
                ref Translation pos,
                ref Rotation rot
            )
            {

                var basepos = this.Translations[ init.PostureEntity ].Value;
                var baserot = this.Rotations[ init.PostureEntity ].Value;

                pos.Value = basepos + math.mul(baserot, pos.Value);
                rot.Value = math.mul(rot.Value, baserot);

                Commands.RemoveComponent<BoneInitializeData>( index, entity );
            }
        }
    }
}
