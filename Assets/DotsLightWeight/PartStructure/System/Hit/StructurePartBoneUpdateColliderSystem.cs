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
    [UpdateAfter(typeof(StructurePartMessageApplySystem))]
    [UpdateBefore(typeof(StructurePartMessageFreeJobSystem))]
    //[UpdateAfter(typeof(StructureEnvelopeWakeupTriggerSystem))]
    public class StructurePartBoneUpdateColliderSystem : DependencyAccessableSystemBase
    {


        public NativeList<Entity> TargetPartBones { get; private set; }



        protected override void OnCreate()
        {
            base.OnCreate();

        }


        protected override void OnUpdate()
        {
            this.TargetPartBones = new NativeList<Entity>(10000, Allocator.TempJob);

            this.Dependency = new JobExecution
            {
                boneParts = this.TargetPartBones.AsDeferredJobArray(),

                lengths = this.GetComponentDataFromEntity<Main.PartLengthData>(isReadOnly: true),

                colliders = this.GetComponentDataFromEntity<PhysicsCollider>(),

                boneInfoBuffers = this.GetBufferFromEntity<PartBone.PartInfoData>(),
                boneColliderBuffers = this.GetBufferFromEntity<PartBone.PartColliderResourceData>(),
            }
            .Schedule(this.TargetPartBones, 32, this.Dependency);
        }


        [BurstCompile]
        struct JobExecution : IJobParallelForDefer
        {

            [DeallocateOnJobCompletion]
            [ReadOnly] public NativeArray<Entity> boneParts;

            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<PhysicsCollider> colliders;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartInfoData> boneInfoBuffers;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartColliderResourceData> boneColliderBuffers;


            [BurstCompile]
            public void Execute(int index)
            {
                var boneEntity = this.boneParts[index];

                var destruction = this.destructions[mainEntity];
                var boneInfoBuffer = this.boneInfoBuffers[boneEntity];
                var boneColliderBuffer = this.boneColliderBuffers[boneEntity];

                var bonePartsDesc = makeBoneParts(hitMessages, boneInfoBuffer.Length);

                TrimBoneColliderBuffer(bonePartsDesc, destruction, boneInfoBuffer, boneColliderBuffer);

                this.colliders[boneEntity] = new PhysicsCollider
                {
                    Value = buildBoneCollider(boneColliderBuffer)
                };
            }



            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                var length = this.lengths[mainEntity];
                var targetBones = makeBones(hitMessages, length.BoneLength);

                foreach (var boneEntity in targetBones)
                {
                    var destruction = this.destructions[mainEntity];
                    var boneInfoBuffer = this.boneInfoBuffers[boneEntity];
                    var boneColliderBuffer = this.boneColliderBuffers[boneEntity];

                    var bonePartsDesc = makeBoneParts(hitMessages, boneInfoBuffer.Length);

                    TrimBoneColliderBuffer(bonePartsDesc, destruction, boneInfoBuffer, boneColliderBuffer);

                    this.colliders[boneEntity] = new PhysicsCollider
                    {
                        Value = buildBoneCollider(boneColliderBuffer)
                    };
                }

                targetBones.Dispose();
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void TrimBoneColliderBuffer(
                NativeArray<int> partIndices,
                Main.PartDestructionData destructions,
                DynamicBuffer<PartBone.PartInfoData> boneInfoBuffer,
                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
            {
                foreach (var i in partIndices)
                {
                    var partid = boneInfoBuffer[i].PartId;

                    if (destructions.IsDestroyed(partid)) continue;

                    boneColliderBuffer.RemoveAtSwapBack(i);
                    boneInfoBuffer.RemoveAtSwapBack(i);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BlobAssetReference<Collider> buildBoneCollider(
                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
            {
                var na = boneColliderBuffer.Reinterpret<CompoundCollider.ColliderBlobInstance>().AsNativeArray();
                return CompoundCollider.Create(na);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            NativeHashSet<Entity> makeBones(
                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages, int boneLength)
            {
                var targetBones = new NativeHashSet<Entity>(boneLength, Allocator.Temp);

                foreach (var msg in hitMessages)
                {
                    targetBones.Add(msg.ColliderEntity);
                }

                return targetBones;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            NativeArray<int> makeBoneParts(
                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages, int bonePartsLength)
            {
                var targetBoneParts = new NativeHashSet<int>(bonePartsLength, Allocator.Temp);

                foreach (var msg in hitMessages)
                {
                    targetBoneParts.Add((int)msg.ColliderChildId);
                }

                var boneParts = targetBoneParts.ToNativeArray(Allocator.Temp);
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
