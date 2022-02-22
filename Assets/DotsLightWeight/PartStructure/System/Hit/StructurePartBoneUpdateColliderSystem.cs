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
        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.allocationSystem = this.World.GetOrCreateSystem<StructurePartMessageAllocationSystem>();
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.freedep = BarrierDependency.Sender.Create<StructurePartMessageFreeJobSystem>(this);
        }


        protected override void OnUpdate()
        {
            using var freeScope = this.freedep.WithDependencyScope();
            using var cmdScope = this.cmddep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                cmd = cmdScope.CommandBuffer.AsParallelWriter(),

                compoundTags = this.GetComponentDataFromEntity<Main.CompoundColliderTag>(isReadOnly: true),
                lengths = this.GetComponentDataFromEntity<Main.PartLengthData>(isReadOnly: true),
                rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true),
                poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true),

                destructions = this.GetComponentDataFromEntity<Main.PartDestructionData>(),

                colliders = this.GetComponentDataFromEntity<PhysicsCollider>(),

                boneInfoBuffers = this.GetBufferFromEntity<PartBone.PartInfoData>(),
                boneColliderBuffers = this.GetBufferFromEntity<PartBone.PartColliderResourceData>(),
            }
            .ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
        }


        [BurstCompile]
        struct JobExecution : HitMessage<PartHitMessage>.IApplyJobExecutionForKey
        {

            public EntityCommandBuffer.ParallelWriter cmd;


            [ReadOnly] public ComponentDataFromEntity<Main.CompoundColliderTag> compoundTags;
            [ReadOnly] public ComponentDataFromEntity<Main.PartLengthData> lengths;
            [ReadOnly] public ComponentDataFromEntity<Rotation> rots;
            [ReadOnly] public ComponentDataFromEntity<Translation> poss;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<Main.PartDestructionData> destructions;

            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<PhysicsCollider> colliders;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartInfoData> boneInfoBuffers;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PartBone.PartColliderResourceData> boneColliderBuffers;



            [BurstCompile]
            public unsafe void Execute(
                int index, Entity mainEntity, NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages)
            {
                if (!this.compoundTags.HasComponent(mainEntity)) return;
                //if (hitMessages.Current.PartId != -1) return;

                Debug.Log($"main {mainEntity}");
                var destruction = this.destructions[mainEntity];


                var wrot = this.rots[mainEntity].Value;
                var wpos = this.poss[mainEntity].Value;

                var ptlength = this.lengths[mainEntity].TotalPartLength;
                using var targets = makeTargetBoneAndPartChildIndices(hitMessages, destruction, ptlength, out var msgCount);

                using var _bones = getUniqueBones(targets, out var bones);
                foreach (var boneEntity in bones)
                {
                    var boneInfoBuffer = this.boneInfoBuffers[boneEntity];
                    var boneColliderBuffer = this.boneColliderBuffers[boneEntity];

                    Debug.Log($"bone {boneEntity}");
                    using var _parts = makeUniqueSortedPartIndexList(targets, boneEntity, msgCount, out var bonePartIdsDesc);

                    //trimBoneColliderBufferAndMarkDestroy_(bonePartsDesc, boneInfoBuffer, boneColliderBuffer, ref destruction);
                    foreach (var i in bonePartIdsDesc)
                    {
                        Debug.Log($"trim indices {i}/{boneInfoBuffer.Length}");
                        var info = boneInfoBuffer[i];
                        var collider = boneColliderBuffer[i];

                        destruction.SetDestroyed(info.PartId);

                        createDebris_(index, wrot, wpos, info, collider);
                        
                        boneColliderBuffer.RemoveAtSwapBack(i);
                        boneInfoBuffer.RemoveAtSwapBack(i);
                    }

                    this.colliders[boneEntity] = new PhysicsCollider
                    {
                        Value = buildBoneCollider(boneColliderBuffer),
                    };
                }


                this.destructions[mainEntity] = destruction;
            }



            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            NativeMultiHashMap<Entity, int> makeTargetBoneAndPartChildIndices(
                NativeMultiHashMap<Entity, PartHitMessage>.Enumerator hitMessages,
                Main.PartDestructionData destructions, int maxPartLength,
                out int msgCount)
            {
                var targets = new NativeMultiHashMap<Entity, int>(maxPartLength, Allocator.Temp);

                var count = 0;
                foreach (var msg in hitMessages)
                {
                    count++;

                    var numSubkeyBits = this.colliders[msg.ColliderEntity].Value.Value.NumColliderKeyBits;
                    msg.ColliderChildKey.PopSubKey(numSubkeyBits, out var childIndex);

                    var partid = this.boneInfoBuffers[msg.ColliderEntity][(int)childIndex].PartId;
                    if (destructions.IsDestroyed(partid)) continue;

                    targets.Add(msg.ColliderEntity, (int)childIndex);
                }

                msgCount = count;
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
                NativeMultiHashMap<Entity, int> partIndices, Entity boneEntity, int msgCount, out NativeSlice<int> uniqueValues)
            {
                var na = new NativeArray<int>(msgCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                
                var count = 0;
                foreach (var item in partIndices.GetValuesForKey(boneEntity))
                {
                    na[count++] = item;
                }

                // HashMapExtension からコード拝借

                var slice = na.Slice(0, count);
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
            struct Desc : IComparer<int>
            {
                public int Compare(int x, int y) => y - x;
            }


            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            //static void trimBoneColliderBufferAndMarkDestroy_(
            //    NativeSlice<int> partIndices,
            //    DynamicBuffer<PartBone.PartInfoData> boneInfoBuffer,
            //    DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer,
            //    ref Main.PartDestructionData destructions)
            //{
            //    foreach (var i in partIndices)
            //    {
            //        Debug.Log($"trim indices {i}/{boneInfoBuffer.Length}");

            //        destructions.SetDestroyed(boneInfoBuffer[i].PartId);

            //        boneColliderBuffer.RemoveAtSwapBack(i);
            //        boneInfoBuffer.RemoveAtSwapBack(i);
            //    }
            //}
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static BlobAssetReference<Collider> buildBoneCollider(
                DynamicBuffer<PartBone.PartColliderResourceData> boneColliderBuffer)
            {
                var na = boneColliderBuffer.Reinterpret<CompoundCollider.ColliderBlobInstance>().AsNativeArray();
                return CompoundCollider.Create(na);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void createDebris_(int uniqueIndex, quaternion wrot, float3 wpos,
                PartBone.PartInfoData info, PartBone.PartColliderResourceData collider)
            {
                var debrisPrefab = info.DebrisPrefab;
                var tf = collider.ColliderInstance.CompoundFromChild;
                var rot = math.mul(wrot, tf.rot);
                var pos = math.rotate(wrot, tf.pos);

                var ent = this.cmd.Instantiate(uniqueIndex, debrisPrefab);
                this.cmd.SetComponent(uniqueIndex, ent, new Rotation { Value = rot });
                this.cmd.SetComponent(uniqueIndex, ent, new Translation { Value = pos });
            }
        }
    }
}
