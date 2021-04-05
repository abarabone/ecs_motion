using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using Unity.Collections.LowLevel.Unsafe;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Abarabone.Structure
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Draw;


    /// <summary>
    /// StructureHitMessageApplySystem_ とセットで使っていたが、やめる。
    /// </summary>
    [DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Simulation.InitializeSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class StructureHitMessageHolderAllocationSystem : SystemBase
    {


        public NativeMultiHashMap<Entity, StructureHitMessage> MsgHolder
        //= new NativeMultiHashMap<Entity, StructureHitMessage>(1000, Allocator.Persistent);
        = new NativeMultiHashMap<Entity, StructureHitMessage>(10000, Allocator.Persistent);

        //public UnsafeMultiHashMap<Entity, StructureHitMessage> MsgHolder
        ////= new NativeMultiHashMap<Entity, StructureHitMessage>(1000, Allocator.Persistent);
        //= new UnsafeMultiHashMap<Entity, StructureHitMessage>(10000, Allocator.Persistent);


        protected override void OnUpdate()
        {

            //this.MsgHolder.Dispose();
            //this.MsgHolder = new NativeMultiHashMap<Entity, StructureHitMessage>(100, Allocator.TempJob);
            this.MsgHolder.Clear();

        }



        protected override void OnDestroy()
        {
            if(this.MsgHolder.IsCreated) this.MsgHolder.Dispose();

            base.OnDestroy();
        }

    }

}
