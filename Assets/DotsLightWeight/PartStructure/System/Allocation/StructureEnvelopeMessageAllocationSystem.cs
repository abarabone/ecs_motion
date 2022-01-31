using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Colider = Unity.Physics.Collider;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;

    using DotsLite.Utility.Log.NoShow;



    public struct EnvelopeHitMessage : IHitMessage
    {

    }

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class StructureEnvelopeMessageAllocationSystem : DependencyAccessableSystemBase, HitMessage<EnvelopeHitMessage>.IRecievable
    {


        public HitMessage<EnvelopeHitMessage>.Reciever Reciever { get; private set; }



        protected override void OnCreate()
        {
            base.OnCreate();

            this.Reciever = new HitMessage<EnvelopeHitMessage>.Reciever();// 10000);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Reciever.Dispose();
        }

        protected override void OnUpdate()
        {
            this.Reciever.Alloc(10000, Allocator.TempJob);
        }
    }

}
