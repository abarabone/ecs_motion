//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Collections.LowLevel.Unsafe;
//using Unity.Entities.LowLevel.Unsafe;
//using Unity.Physics;
//using System.Collections.Generic;
//using Unity.Jobs.LowLevel.Unsafe;
//using System.Runtime.CompilerServices;
//using UnityEngine;

//using Collider = Unity.Physics.Collider;

//namespace DotsLite.Structure
//{
//    using DotsLite.Dependency;

//    using DotsLite.Utility.Log.NoShow;


//    //[DisableAutoCreation]
//    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
//    //[UpdateAfter(typeof(StructurePartMessageAllocationSystem))]
//    [UpdateAfter(typeof(StructureEnvelopeMessageApplySystem))]
//    [UpdateBefore(typeof(StructurePartMessageFreeJobSystem))]
//    //[UpdateAfter(typeof(StructureEnvelopeWakeupTriggerSystem))]
//    public class StructurePartBoneUpdateColliderSystem : DependencyAccessableSystemBase
//    {


//        public NativeList<Entity> TargetPartBones { get; private set; }


//        //StructurePartMessageAllocationSystem allocationSystem;

//        //BarrierDependency.Sender freedep;
//        //CommandBufferDependency.Sender cmddep;


//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            //this.allocationSystem = this.World.GetOrCreateSystem<StructurePartMessageAllocationSystem>();
//            ////this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
//            //this.freedep = BarrierDependency.Sender.Create<StructurePartMessageFreeJobSystem>(this);
//        }


//        protected override void OnUpdate()
//        {
//            //using var freeScope = this.freedep.WithDependencyScope();
//            //using var cmdScope = this.cmddep.WithDependencyScope();


//            //var cmd = cmdScope.CommandBuffer.AsParallelWriter();

//            this.Dependency = new JobExecution
//            {
//                //destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(isReadOnly: true),
//                lengths = this.GetComponentDataFromEntity<Main.PartLengthData>(isReadOnly: true),

//                colliders = this.GetComponentDataFromEntity<PhysicsCollider>(),

//                boneInfoBuffers = this.GetBufferFromEntity<PartBone.PartInfoData>(),
//                boneColliderBuffers = this.GetBufferFromEntity<PartBone.PartColliderResourceData>(),
//            }
//            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
//        }


//        [BurstCompile]
//        public struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
//        {

//            [ReadOnly] public ComponentDataFromEntity<Main.PartDestructionData> destructions;
//            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;

//            [NativeDisableParallelForRestriction]
//            public ComponentDataFromEntity<PhysicsCollider> colliders;

//            [NativeDisableParallelForRestriction]
//            public BufferFromEntity<PartBone.PartInfoData> boneInfoBuffers;
//            [NativeDisableParallelForRestriction]
//            public BufferFromEntity<PartBone.PartColliderResourceData> boneColliderBuffers;


//            [BurstCompile]
//            public unsafe void Execute(
//                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
//            {
//                var length = this.lengths[mainEntity];
//                var targetBones = makeBones(hitMessages, length.BoneLength);

//                foreach (var boneEntity in targetBones)
//                {
//                    var destruction = this.destructions[mainEntity];
//                    var boneInfoBuffer = this.boneInfoBuffers[boneEntity];
//                    var boneColliderBuffer = this.boneColliderBuffers[boneEntity];

//                    var bonePartsDesc = makeBoneParts(hitMessages, boneInfoBuffer.Length);

//                    TrimBoneColliderBuffer(bonePartsDesc, destruction, boneInfoBuffer, boneColliderBuffer);

//                    this.colliders[boneEntity] = new PhysicsCollider
//                    {
//                        Value = buildBoneCollider(boneColliderBuffer)
//                    };
//                }

//                targetBones.Dispose();
//            }
//            [MethodImpl(MethodImplOptions.AggressiveInlining)]
//            public void TrimBoneColliderBuffer(
//                NativeArray<int> partIndices,
//                Main.PartDestructionData destructions,
//                DynamicBuffer<PartBone.PartInfoData> boneInfoBuffer,
//                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
//            {
//                foreach (var i in partIndices)
//                {
//                    var partid = boneInfoBuffer[i].PartId;

//                    if (destructions.IsDestroyed(partid)) continue;

//                    boneColliderBuffer.RemoveAtSwapBack(i);
//                    boneInfoBuffer.RemoveAtSwapBack(i);
//                }
//            }
//            [MethodImpl(MethodImplOptions.AggressiveInlining)]
//            public BlobAssetReference<Collider> buildBoneCollider(
//                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
//            {
//                var na = boneColliderBuffer.Reinterpret<CompoundCollider.ColliderBlobInstance>().AsNativeArray();
//                return CompoundCollider.Create(na);
//            }

//            [MethodImpl(MethodImplOptions.AggressiveInlining)]
//            NativeHashSet<Entity> makeBones(
//                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages, int boneLength)
//            {
//                var targetBones = new NativeHashSet<Entity>(boneLength, Allocator.Temp);

//                foreach (var msg in hitMessages)
//                {
//                    targetBones.Add(msg.ColliderEntity);
//                }

//                return targetBones;
//            }
//            [MethodImpl(MethodImplOptions.AggressiveInlining)]
//            NativeArray<int> makeBoneParts(
//                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages, int bonePartsLength)
//            {
//                var targetBoneParts = new NativeHashSet<int>(bonePartsLength, Allocator.Temp);

//                foreach (var msg in hitMessages)
//                {
//                    targetBoneParts.Add((int)msg.ColliderChildId);
//                }

//                var boneParts = targetBoneParts.ToNativeArray(Allocator.Temp);
//                boneParts.Sort(new Desc());

//                return boneParts;
//            }
//            struct Desc : IComparer<int>
//            {
//                public int Compare(int x, int y) => y - x;
//            }

//        }
//    }
//}
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.LowLevel.Unsafe;
using Unity.Physics;
using System.Collections.Generic;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Collider = Unity.Physics.Collider;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;

    using DotsLite.Utility.Log.NoShow;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateAfter(typeof(StructureEnvelopeMessageApplySystem))]
    [UpdateBefore(typeof(StructurePartMessageApplySystem))]
    [UpdateBefore(typeof(StructurePartMessageFreeJobSystem))]
    //[UpdateAfter(typeof(StructureEnvelopeWakeupTriggerSystem))]
    public class StructurePartBoneUpdateColliderSystem : DependencyAccessableSystemBase
    {


        StructurePartMessageAllocationSystem allocationSystem;

        BarrierDependency.Sender freedep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.allocationSystem = this.World.GetOrCreateSystem<StructurePartMessageAllocationSystem>();
            this.freedep = BarrierDependency.Sender.Create<StructurePartMessageFreeJobSystem>(this);
        }


        protected override void OnUpdate()
        {
            using var freeScope = this.freedep.WithDependencyScope();


            this.Dependency = new JobExecution
            {
                destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(isReadOnly: true),
                compoundTags = this.GetComponentDataFromEntity<Main.CompoundColliderTag>(isReadOnly: true),
                lengths = this.GetComponentDataFromEntity<Main.PartLengthData>(isReadOnly: true),

                partBones = this.GetComponentDataFromEntity<PartBone.LengthData>(isReadOnly: true),
                colliders = this.GetComponentDataFromEntity<PhysicsCollider>(),

                boneInfoBuffers = this.GetBufferFromEntity<PartBone.PartInfoData>(),
                boneColliderBuffers = this.GetBufferFromEntity<PartBone.PartColliderResourceData>(),
            }
            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            [ReadOnly] public ComponentDataFromEntity<Main.PartDestructionData> destructions;
            [ReadOnly] public ComponentDataFromEntity<Main.CompoundColliderTag> compoundTags;
            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;

            [ReadOnly] public ComponentDataFromEntity<PartBone.LengthData> partBones;
            [NativeDisableParallelForRestriction]
            [WriteOnly] public ComponentDataFromEntity<PhysicsCollider> colliders;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartInfoData> boneInfoBuffers;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartColliderResourceData> boneColliderBuffers;



            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                if (!this.compoundTags.HasComponent(mainEntity)) return;


                var length = this.lengths[mainEntity];
                var destruction = this.destructions[mainEntity];
                using var targets = makeTargetBoneAndPartChildIndices(destruction, hitMessages, length.TotalPartLength);

                using var bones = targets.GetKeyArray(Allocator.Temp);
                foreach (var boneEntity in bones)
                {
                    var boneInfoBuffer = this.boneInfoBuffers[boneEntity];
                    var boneColliderBuffer = this.boneColliderBuffers[boneEntity];

                    var indexEnumerator = targets.GetValuesForKey(boneEntity);
                    using var bonePartsDesc = makeUniqueSortedPartIndexList(indexEnumerator, boneInfoBuffer.Length);

                    //TrimBoneColliderBuffer(bonePartsDesc, boneInfoBuffer, boneColliderBuffer);

                    //this.colliders[boneEntity] = new PhysicsCollider
                    //{
                    //    Value = buildBoneCollider(boneColliderBuffer)
                    //};
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void trimBoneColliderBuffer(
                NativeArray<int> partIndices,
                DynamicBuffer<PartBone.PartInfoData> boneInfoBuffer,
                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
            {
                foreach (var i in partIndices)
                {
                    var partid = boneInfoBuffer[i].PartId;

                    boneColliderBuffer.RemoveAtSwapBack(i);
                    boneInfoBuffer.RemoveAtSwapBack(i);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            BlobAssetReference<Collider> buildBoneCollider(
                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
            {
                var na = boneColliderBuffer.Reinterpret<CompoundCollider.ColliderBlobInstance>().AsNativeArray();
                return CompoundCollider.Create(na);
            }

            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //NativeHashSet<Entity> makeTargetBoneList(
            //    Main.PartDestructionData destructions,
            //    NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages, int boneLength)
            //{
            //    var targetBones = new NativeHashSet<Entity>(boneLength, Allocator.Temp);

            //    foreach (var msg in hitMessages)
            //    {
            //        if (destructions.IsDestroyed(msg.PartId)) continue;

            //        targetBones.Add(msg.ColliderEntity);
            //    }

            //    return targetBones;
            //}
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //NativeArray<int> makeSortedBonePartList(
            //    NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages, int bonePartsLength)
            //{
            //    var targetBoneParts = new NativeHashSet<int>(bonePartsLength, Allocator.Temp);

            //    foreach (var msg in hitMessages)
            //    {
            //        targetBoneParts.Add((int)msg.ColliderChildId);
            //    }

            //    var boneParts = targetBoneParts.ToNativeArray(Allocator.Temp);
            //    boneParts.Sort(new Desc());

            //    targetBoneParts.Dispose();
            //    return boneParts;
            //}
            //struct Desc : IComparer<int>
            //{
            //    public int Compare(int x, int y) => y - x;
            //}

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            NativeMultiHashMap<Entity, int> makeTargetBoneAndPartChildIndices(
                Main.PartDestructionData destructions,
                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages,
                int maxPartLength)
            {
                var targets = new NativeMultiHashMap<Entity, int>(maxPartLength, Allocator.Temp);

                foreach (var msg in hitMessages)
                {
                    if (destructions.IsDestroyed(msg.PartId)) continue;

                    var numSubkeyBits = this.partBones[msg.ColliderEntity].NumSubkeyBits;
                    msg.ColliderChildKey.PopSubKey(numSubkeyBits, out var childIndex);
                    targets.Add(msg.ColliderEntity, (int)childIndex);
                }

                return targets;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            NativeArray<int> makeUniqueSortedPartIndexList(
                NativeMultiHashMap<Entity, int>.Enumerator partIndices, int bonePartsLength)
            {
                using var uniqueIndices = new NativeHashSet<int>(bonePartsLength, Allocator.Temp);

                foreach (var idx in partIndices)
                {
                    Debug.Log($"raw indices {idx}");
                    uniqueIndices.Add(idx);
                }

                using var boneParts = uniqueIndices.ToNativeArray(Allocator.Temp);
                boneParts.Sort(new Desc());

                return boneParts;
            }
            struct Desc : IComparer<int>
            {
                public int Compare(int x, int y) => y - x;
            }
        }
    }
}
