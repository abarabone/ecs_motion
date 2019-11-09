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
using Abss.Character;

namespace Abss.Motion
{

    //[DisableAutoCreation]
    [UpdateAfter(typeof( StreamToBoneSystem ) )]
    [UpdateInGroup(typeof(MotionSystemGroup))]
    public class BoneTransformSystem : JobComponentSystem
    {

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new BoneTransformJob
            {
                BoneRelationLinkers = this.GetComponentDataFromEntity<BoneRelationLinkData>( isReadOnly: true ),
                //BoneStreamLinkers = this.GetComponentDataFromEntity<BoneStreamLinkData>( isReadOnly: true ),
                BoneIndexers = this.GetComponentDataFromEntity<BoneIndexData>(),//
                BonePositions = this.GetComponentDataFromEntity<Translation>(),
                BoneRotations = this.GetComponentDataFromEntity<Rotation>(),
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }


        [BurstCompile]
        struct BoneTransformJob : IJobForEach<PostureNeedTransformTag, PostureLinkData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<BoneRelationLinkData>    BoneRelationLinkers;
            //[ReadOnly]
            //public ComponentDataFromEntity<BoneStreamLinkData>      BoneStreamLinkers;

            [ReadOnly]
            public ComponentDataFromEntity<BoneIndexData>   BoneIndexers;//
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation>     BonePositions;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Rotation>        BoneRotations;

            public void Execute(
                [ReadOnly] ref PostureNeedTransformTag tag,
                [ReadOnly] ref PostureLinkData linker
            )
            {
                for( var ent = linker.BoneRelationTop; ent != Entity.Null; ent = this.BoneRelationLinkers[ent].NextBoneEntity )
                {
                    var parent = this.BoneRelationLinkers[ ent ].ParentBoneEntity;

                    var ppos = this.BonePositions[ parent ].Value;
                    var prot = this.BoneRotations[ parent ].Value;

                    var lpos = this.BonePositions[ ent ].Value;
                    var lrot = this.BoneRotations[ ent ].Value;

                    var pos = math.mul( prot, lpos ) + ppos;
                    var rot = math.mul( prot, lrot );

                    this.BonePositions[ ent ] = new Translation { Value = pos };
                    this.BoneRotations[ ent ] = new Rotation { Value = rot };
                }
            }
        }
    }
    
}
