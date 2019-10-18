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

            var bonePositions = this.GetComponentDataFromEntity<Translation>();// isReadOnly: true );
            var boneRotations = this.GetComponentDataFromEntity<Rotation>();// isReadOnly: true );

            inputDeps = new BoneLv01TransformJob<BoneLv01LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv02LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv03LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv04LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv05LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv06LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv07LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv08LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            inputDeps = new BoneLv01TransformJob<BoneLv09LinkData>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            return inputDeps;
        }


        [BurstCompile]
        struct BoneLv01TransformJob<T> : IJobForEachWithEntity<T>//, Translation, Rotation>
            where T:struct,IComponentData,IBoneLvLinkData
        {

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Translation> BonePositions;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Rotation> BoneRotations;


            //public void Execute(
            //    [ReadOnly] ref BoneLv01LinkData lv,
            //    ref Translation pos,
            //    ref Rotation rot
            //)
            //{
            //    var ppos = this.BonePositions[ lv.ParentBoneEntity ];
            //    var prot = this.BoneRotations[ lv.ParentBoneEntity ];

            //    pos.Value = math.mul( prot.Value, pos.Value ) + ppos.Value;
            //    rot.Value = math.mul( prot.Value, rot.Value );
            //}

            public void Execute( Entity entity, int index, ref T linker )
            {
                var parent = linker.GetParentBoneEntity;

                var ppos = this.BonePositions[ parent ].Value;
                var prot = this.BoneRotations[ parent ].Value;

                var lpos = this.BonePositions[ entity ].Value;
                var lrot = this.BoneRotations[ entity ].Value;

                var pos = math.mul( prot, lpos ) + ppos;
                var rot = math.mul( prot, lrot );

                this.BonePositions[ entity ] = new Translation { Value = pos };
                this.BoneRotations[ entity ] = new Rotation { Value = rot };
            }
        }
    }

}
