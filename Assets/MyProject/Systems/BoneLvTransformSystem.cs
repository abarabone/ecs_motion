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
using Abss.Object;

namespace Abss.Motion
{

    //[DisableAutoCreation]
    [UpdateAfter( typeof( StreamToBoneSystem ) )]
    [UpdateInGroup( typeof( MotionSystemGroup ) )]
    public class BoneLvTransformSystem : JobComponentSystem
    {

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new BoneLv01TransformJob
            {
                BonePositions = this.GetComponentDataFromEntity<Translation>(),// isReadOnly: true ),
                BoneRotations = this.GetComponentDataFromEntity<Rotation>(),// isReadOnly: true ),
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }


        [BurstCompile]
        struct BoneLv01TransformJob : IJobForEach<BoneLv01Data, Translation, Rotation>
        {

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> BonePositions;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Rotation> BoneRotations;


            public void Execute(
                [ReadOnly] ref BoneLv01Data lv,
                ref Translation pos,
                ref Rotation rot
            )
            {
                var ppos = this.BonePositions[ lv.ParentBoneEntity ];
                var prot = this.BoneRotations[ lv.ParentBoneEntity ];

                pos.Value = math.mul( prot.Value, pos.Value ) + ppos.Value;
                rot.Value = math.mul( prot.Value, rot.Value );
            }
        }
    }

}
