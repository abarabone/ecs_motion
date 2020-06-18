﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;


using Abarabone.Authoring;
using Abarabone.SystemGroup;
using Abarabone.Utilities;
using Abarabone.Geometry;
using Abarabone.Character;

namespace Abarabone.Motion
{

    //[DisableAutoCreation]
    [UpdateAfter( typeof( StreamToBoneSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv01TransformSystem : _BoneTransformLeveledSystem<BoneLv01LinkData>
    { }
    [UpdateAfter( typeof( BoneLv01TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv02TransformSystem : _BoneTransformLeveledSystem<BoneLv02LinkData>
    { }
    [UpdateAfter( typeof( BoneLv02TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv03TransformSystem : _BoneTransformLeveledSystem<BoneLv03LinkData>
    { }
    [UpdateAfter( typeof( BoneLv03TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv04TransformSystem : _BoneTransformLeveledSystem<BoneLv04LinkData>
    { }
    [UpdateAfter( typeof( BoneLv04TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv05TransformSystem : _BoneTransformLeveledSystem<BoneLv05LinkData>
    { }
    [UpdateAfter( typeof( BoneLv05TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv06TransformSystem : _BoneTransformLeveledSystem<BoneLv06LinkData>
    { }
    [UpdateAfter( typeof( BoneLv06TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv07TransformSystem : _BoneTransformLeveledSystem<BoneLv07LinkData>
    { }
    [UpdateAfter( typeof( BoneLv07TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv08TransformSystem : _BoneTransformLeveledSystem<BoneLv08LinkData>
    { }
    [UpdateAfter( typeof( BoneLv08TransformSystem ) )]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
    public class BoneLv09TransformSystem : _BoneTransformLeveledSystem<BoneLv09LinkData>
    { }

    public abstract class _BoneTransformLeveledSystem<T> : JobComponentSystem
        where T:struct,IComponentData,IBoneLvLinkData
    {

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var bonePositions = this.GetComponentDataFromEntity<Translation>();// isReadOnly: true );
            var boneRotations = this.GetComponentDataFromEntity<Rotation>();// isReadOnly: true );

            inputDeps = new BoneTransformLeveledJob<T>
            {
                BonePositions = bonePositions,
                BoneRotations = boneRotations,
            }
            .Schedule( this, inputDeps );
            
            return inputDeps;
        }


    }

    [BurstCompile]
    public struct BoneTransformLeveledJob<T> : IJobForEachWithEntity<T>//, Translation, Rotation>
        where T : struct, IComponentData, IBoneLvLinkData
    {

        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Translation> BonePositions;
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Rotation> BoneRotations;
        
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