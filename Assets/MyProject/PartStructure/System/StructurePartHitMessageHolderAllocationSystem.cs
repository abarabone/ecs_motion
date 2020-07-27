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

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Abarabone.Structure
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Draw;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.MonolithicBoneTransform.MonolithicBoneTransformSystemGroup))]
    public class StructureHitMessageHolderAllocationSystem : SystemBase
    {


        public NativeMultiHashMap<Entity, StructureHitMessage> MsgHolder
             = new NativeMultiHashMap<Entity, StructureHitMessage>(10000, Allocator.Persistent);


        protected override void OnUpdate()
        {

            //this.MsgHolder = new NativeMultiHashMap<Entity, StructurePartHitMessage>(100, Allocator.TempJob);
            this.MsgHolder.Clear();

        }



        protected override void OnDestroy()
        {
            if(this.MsgHolder.IsCreated) this.MsgHolder.Dispose();

            base.OnDestroy();
        }

    }

}
