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
                destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(),//isReadOnly: true),
                compoundTags = this.GetComponentDataFromEntity<Main.CompoundColliderTag>(isReadOnly: true),
                lengths = this.GetComponentDataFromEntity<Main.PartLengthData>(isReadOnly: true),

                //partBones = this.GetComponentDataFromEntity<PartBone.LengthData>(isReadOnly: true),
                colliders = this.GetComponentDataFromEntity<PhysicsCollider>(),

                boneInfoBuffers = this.GetBufferFromEntity<PartBone.PartInfoData>(),
                boneColliderBuffers = this.GetBufferFromEntity<PartBone.PartColliderResourceData>(),

                //time = Time.ElapsedTime,
            }
            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            [ReadOnly] public ComponentDataFromEntity<Main.CompoundColliderTag> compoundTags;
            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Main.PartDestructionData> destructions;

            //public ComponentDataFromEntity<PartBone.LengthData> partBones;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<PhysicsCollider> colliders;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartInfoData> boneInfoBuffers;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartColliderResourceData> boneColliderBuffers;


            //public double time;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                if (!this.compoundTags.HasComponent(mainEntity)) return;
                //if (hitMessages.Current.PartId != -1) return;

                Debug.Log($"first {mainEntity}");
                var destruction = this.destructions[mainEntity];


                var length = this.lengths[mainEntity];
                using var targets = makeTargetBoneAndPartChildIndices(destruction, hitMessages, length.TotalPartLength);

                using var _bones = getUniqueBones(targets, out var bones);
                foreach (var boneEntity in bones)
                {
                    Debug.Log($"bone {boneEntity}");
                    var boneInfoBuffer = this.boneInfoBuffers[boneEntity];
                    var boneColliderBuffer = this.boneColliderBuffers[boneEntity];
                    using var _parts = makeUniqueSortedPartIndexList(targets, boneEntity, boneInfoBuffer.Length, out var bonePartsDesc);

                    trimBoneColliderBufferAndMarkDestroy_(bonePartsDesc, boneInfoBuffer, boneColliderBuffer, ref destruction);

                    this.colliders[boneEntity] = new PhysicsCollider
                    {
                        Value = buildBoneCollider(boneColliderBuffer),
                    };
                }


                this.destructions[mainEntity] = destruction;
            }



            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            NativeMultiHashMap<Entity, int> makeTargetBoneAndPartChildIndices(
                Main.PartDestructionData destructions,
                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages,
                int maxPartLength)
            {
                var targets = new NativeMultiHashMap<Entity, int>(maxPartLength, Allocator.Temp);

                foreach (var msg in hitMessages)
                {
                    var numSubkeyBits = this.colliders[msg.ColliderEntity].Value.Value.NumColliderKeyBits;
                    msg.ColliderChildKey.PopSubKey(numSubkeyBits, out var childIndex);

                    var partid = this.boneInfoBuffers[msg.ColliderEntity][(int)childIndex].PartId;
                    if (destructions.IsDestroyed(partid)) continue;

                    targets.Add(msg.ColliderEntity, (int)childIndex);
                }

                return targets;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            NativeArray<Entity> getUniqueBones(NativeMultiHashMap<Entity, int> targets, out NativeSlice<Entity> uniqueKeys)
            {
                var (keys, keylength) = targets.GetUniqueKeyArray(Allocator.Temp);
                uniqueKeys = keys.Slice(0, keylength);
                return keys;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static NativeArray<int> makeUniqueSortedPartIndexList(
                NativeMultiHashMap<Entity, int> partIndices, Entity boneEntity, int maxPartLength, out NativeSlice<int> uniqueValues)
            {
                var na = new NativeArray<int>(maxPartLength, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var i = 0;
                foreach (var item in partIndices.GetValuesForKey(boneEntity))
                {
                    Debug.Log($"item {item}");
                    na[i++] = item;
                }
                var slice = na.Slice(0, i);
                slice.Sort(new Desc());
                uniqueValues = slice.Slice(0, unique(slice));
                return na;

                static int unique(NativeSlice<int> s)
                {
                    int first = 0;
                    int last = s.Length;
                    var result = first;
                    while (++first != last)
                    {
                        if (!s[result].Equals(s[first]))
                        {
                            s[++result] = s[first];
                        }
                    }

                    return ++result;
                }
            }
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //static NativeArray<int> makeUniqueSortedPartIndexList(
            //    NativeMultiHashMap<Entity, int>.Enumerator partIndices, int bonePartsLength)
            //{
            //    using var uniqueIndices = new NativeHashSet<int>(bonePartsLength, Allocator.Temp);

            //    foreach (var idx in partIndices)
            //    {
            //        Debug.Log($"raw indices {idx}");
            //        uniqueIndices.Add(idx);
            //    }

            //    using var boneParts = uniqueIndices.ToNativeArray(Allocator.Temp);
            //    boneParts.Sort(new Desc());

            //    return boneParts;
            //}
            struct Desc : IComparer<int>
            {
                public int Compare(int x, int y) => y - x;
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void trimBoneColliderBufferAndMarkDestroy_(
                NativeSlice<int> partIndices,
                DynamicBuffer<PartBone.PartInfoData> boneInfoBuffer,
                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer,
                ref Main.PartDestructionData destructions)
            {
                foreach (var i in partIndices)
                {
                    Debug.Log($"trim indices {i}/{boneInfoBuffer.Length}");

                    destructions.SetDestroyed(boneInfoBuffer[i].PartId);

                    boneColliderBuffer.RemoveAtSwapBack(i);
                    boneInfoBuffer.RemoveAtSwapBack(i);
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static BlobAssetReference<Collider> buildBoneCollider(
                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
            {
                var na = boneColliderBuffer.Reinterpret<CompoundCollider.ColliderBlobInstance>().AsNativeArray();
                return CompoundCollider.Create(na);
            }


        }
    }
}
