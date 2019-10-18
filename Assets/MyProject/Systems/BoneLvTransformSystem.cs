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
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv01TransformSystem : _BoneLvTransformSystem<BoneLv01LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv02TransformSystem : _BoneLvTransformSystem<BoneLv02LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv03TransformSystem : _BoneLvTransformSystem<BoneLv03LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv04TransformSystem : _BoneLvTransformSystem<BoneLv04LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv05TransformSystem : _BoneLvTransformSystem<BoneLv05LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv06TransformSystem : _BoneLvTransformSystem<BoneLv06LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv07TransformSystem : _BoneLvTransformSystem<BoneLv07LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv08TransformSystem : _BoneLvTransformSystem<BoneLv08LinkData>
    //{ }
    //[UpdateAfter( typeof( StreamToBoneSystem ) )]
    //[UpdateInGroup( typeof( MotionSystemGroup ) )]
    //public class BoneLv09TransformSystem : _BoneLvTransformSystem<BoneLv09LinkData>
    //{ }

    public class _BoneLvTransformSystem<T> : JobComponentSystem
        where T:struct,IComponentData,IBoneLvLinkData
    {

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var bonePositions = this.GetComponentDataFromEntity<Translation>();// isReadOnly: true );
            var boneRotations = this.GetComponentDataFromEntity<Rotation>();// isReadOnly: true );

            inputDeps = new BoneLvTransformJob<T>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv01LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv02LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv03LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv04LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv05LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv06LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv07LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv08LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            //inputDeps = new BoneLvTransformJob<BoneLv09LinkData>
            //{
            //    BonePositions = bonePositions,
            //    BoneRotations = boneRotations,
            //}
            //.Schedule( this, inputDeps );

            return inputDeps;
        }


        [BurstCompile]
        struct BoneLvTransformJob<T> : IJobForEachWithEntity<T>//, Translation, Rotation>
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

    public class _BoneLvTransformSystem<BoneLv09LinkData>;

}
