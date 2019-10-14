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
    [UpdateAfter(typeof( StreamToBoneSystem ) )]
    [UpdateInGroup(typeof(MotionSystemGroup))]
    public class BoneTransformSystem : JobComponentSystem
    {

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new BoneTransformJob
            {
                BoneRelationLinkers = this.GetComponentDataFromEntity<BoneRelationLinkData>( isReadOnly: true ),
                BoneStreamLinkers = this.GetComponentDataFromEntity<BoneStreamLinkData>( isReadOnly: true ),
                BonePoss = this.GetComponentDataFromEntity<Translation>(),
                BoneRots = this.GetComponentDataFromEntity<Rotation>(),
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }


        [BurstCompile]
        struct BoneTransformJob : IJobForEach<PostureNeedTransformTag, PostureLinkData>
        {

            [ReadOnly]
            public ComponentDataFromEntity<BoneRelationLinkData>    BoneRelationLinkers;
            [ReadOnly]
            public ComponentDataFromEntity<BoneStreamLinkData>      BoneStreamLinkers;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation>             BonePoss;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Rotation>                BoneRots;

            public void Execute(
                [ReadOnly] ref PostureNeedTransformTag tag,
                [ReadOnly] ref PostureLinkData linker
            )
            {
                for( var ent = linker.BoneRelationTop; ent != Entity.Null; ent = this.BoneRelationLinkers[ent].NextEntity )
                {
                    var parent = this.BoneRelationLinkers[ ent ].ParentBoneEntity;

                    var ppos = this.BonePoss[ parent ].Value;
                    var prot = this.BoneRots[ parent ].Value;

                    var lpos = this.BonePoss[ ent ].Value;
                    var lrot = this.BonePoss[ ent ].Value;

                    var mpos = math.mul( prot, lpos ) + ppos;
                    var mrot = math.mul( prot, lrot );

                    this.BonePoss[ ent ] = new Translation { Value = mpos };
                    this.BoneRots[ ent ] = new Rotation { Value = mrot.As_float4() };
                }
            }
        }
    }


    [UpdateAfter( typeof( MotionProgressSystem ) )]
    [UpdateInGroup( typeof( MotionSystemGroup ) )]
    public class StreamToBoneSystem : JobComponentSystem
    {


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new StreamToBoneJob
            {
                StreamValues = this.GetComponentDataFromEntity<StreamInterpolatedData>( isReadOnly: true ),
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile]
        public struct StreamToBoneJob : IJobForEach<BoneStreamLinkData, Translation, Rotation>
        {

            [ReadOnly]
            public ComponentDataFromEntity<StreamInterpolatedData> StreamValues;


            public void Execute(
                [ReadOnly]  ref BoneStreamLinkData streamLinker,
                [WriteOnly] ref Translation pos,
                [WriteOnly] ref Rotation rot
            )
            {

                pos.Value = this.StreamValues[ streamLinker.PositionStreamEntity ].Value.As_float3();
                rot.Value = this.StreamValues[ streamLinker.RotationStreamEntity ].Value;

            }
        }

    }

}
