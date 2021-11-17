using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections.LowLevel;

namespace DotsLite.Dependency
{

    // 特に必要なかったが、まぁマーキングとしてつくっとこう
    public interface IHitMessage
    { }



    public static partial class HitMessage<THitMessage>
        where THitMessage : struct, IHitMessage
    {


        [BurstCompile]
        public struct ParallelWriter
        {
            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            NativeList<Entity>.ParallelWriter nl;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            NativeMultiHashMap<Entity, THitMessage>.ParallelWriter hm;

            [NativeDisableParallelForRestriction]
            [NativeDisableContainerSafetyRestriction]
            NativeHashSet<Entity>.ParallelWriter uk;

            public ParallelWriter(
                ref NativeList<Entity> nl,
                ref NativeMultiHashMap<Entity, THitMessage> hm,
                ref NativeHashSet<Entity> uk)
            {
                this.nl = nl.AsParallelWriter();
                this.hm = hm.AsParallelWriter();
                this.uk = uk.AsParallelWriter();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(Entity entity, THitMessage hitMessage)
            {
                if (this.uk.Add(entity)) this.nl.AddNoResize(entity);
                this.hm.Add(entity, hitMessage);
            }
        }

    }


}
