using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;


namespace Abarabone.Model
{

    using Abarabone.Authoring;
    using Abarabone.SystemGroup;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Character;
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Unity.Entities.CodeGeneratedJobForEach;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MotionBoneTransform.MotionSystemGroup))]
    [UpdateAfter(typeof(StreamToBoneSystem))]
    public class BoneTransformLeveldSystem : SystemBase
    {
        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>();
            var rots = this.GetComponentDataFromEntity<Rotation>();

            var deltaTime = this.Time.DeltaTime;


            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv01LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv02LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv03LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv04LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv05LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv06LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv07LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv08LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

            this.Entities
                .WithBurst()
                .WithReadOnly(poss)
                .WithReadOnly(rots)
                .ForEach(
                    (
                        Entity entity,
                        ref Translation pos, ref Rotation rot,
                        in Bone.Lv09LinkData link, in Bone.LocalValueData local
                    ) =>
                    {
                        var parent = link.ParentBoneEntity;
                        var parentpos = poss[parent];
                        var parentrot = rots[parent];

                        BoneUtility.BoneTransform(in parentpos, in parentrot, in local, ref pos, ref rot);
                    }
                )
                .ScheduleParallel();

        }

    }


}

//    //[DisableAutoCreation]
//    [UpdateAfter( typeof( StreamToBoneSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv01TransformSystem : _BoneTransformLeveledSystem<Bone.Lv01LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv01TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv02TransformSystem : _BoneTransformLeveledSystem<Bone.Lv02LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv02TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv03TransformSystem : _BoneTransformLeveledSystem<Bone.Lv03LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv03TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv04TransformSystem : _BoneTransformLeveledSystem<Bone.Lv04LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv04TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv05TransformSystem : _BoneTransformLeveledSystem<Bone.Lv05LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv05TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv06TransformSystem : _BoneTransformLeveledSystem<Bone.Lv06LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv06TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv07TransformSystem : _BoneTransformLeveledSystem<Bone.Lv07LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv07TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv08TransformSystem : _BoneTransformLeveledSystem<Bone.Lv08LinkData>
//    { }
//    [UpdateAfter( typeof( Bone.Lv08TransformSystem ) )]
//    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.Motion.MotionSystemGroup ) )]
//    public class Bone.Lv09TransformSystem : _BoneTransformLeveledSystem<Bone.Lv09LinkData>
//    { }

//    public abstract class _BoneTransformLeveledSystem<T> : JobComponentSystem
//        where T:struct,IComponentData,Bone.ILvLinkData
//    {

//        protected override JobHandle OnUpdate( JobHandle inputDeps )
//        {

//            var bonePositions = this.GetComponentDataFromEntity<Translation>();// isReadOnly: true );
//            var boneRotations = this.GetComponentDataFromEntity<Rotation>();// isReadOnly: true );

//            inputDeps = new BoneTransformLeveledJob<T>
//            {
//                BonePositions = bonePositions,
//                BoneRotations = boneRotations,
//            }
//            .Schedule( this, inputDeps );

//            return inputDeps;
//        }


//    }

//    [BurstCompile]
//    public struct BoneTransformLeveledJob<T> : IJobForEachWithEntity<T>//, Translation, Rotation>
//        where T : struct, IComponentData, Bone.ILvLinkData
//    {

//        [NativeDisableParallelForRestriction]
//        public ComponentDataFromEntity<Translation> BonePositions;
//        [NativeDisableParallelForRestriction]
//        public ComponentDataFromEntity<Rotation> BoneRotations;

//        public void Execute( Entity entity, int index, ref T linker )
//        {
//            var parent = linker.GetParentBoneEntity;

//            var ppos = this.BonePositions[ parent ].Value;
//            var prot = this.BoneRotations[ parent ].Value;

//            var lpos = this.BonePositions[ entity ].Value;
//            var lrot = this.BoneRotations[ entity ].Value;

//            var pos = math.mul( prot, lpos ) + ppos;
//            var rot = math.mul( prot, lrot );

//            this.BonePositions[ entity ] = new Translation { Value = pos };
//            this.BoneRotations[ entity ] = new Rotation { Value = rot };
//        }
//    }
//}
