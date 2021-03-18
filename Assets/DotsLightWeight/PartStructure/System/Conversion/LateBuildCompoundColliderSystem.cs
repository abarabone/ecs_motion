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
using System;
using Unity.Jobs.LowLevel.Unsafe;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Abarabone.Structure.Authoring
{

    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class LateBuildCompoundColliderSystem : GameObjectConversionSystem
    {

        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            //this.Entities.ForEach
            //(
            //    (Entity e, StructureBuildingModelAuthoring.BuildCompoundColliderLateTag c) =>
            //    {
            //        foreach (var tf in c.GetComponentsInChildren<StructurePartAuthoring>())
            //        {
            //            Debug.Log(tf.name);
            //            var ent = this.GetPrimaryEntity(tf);

            //            em.RemoveComponent<LocalToParent>(ent);
            //            em.RemoveComponent<LocalToWorld>(ent);
            //            em.RemoveComponent<PreviousParent>(ent);
            //            em.RemoveComponent<Parent>(ent);
            //            em.RemoveComponent<Child>(ent);

            //        }
            //    }
            //);
        }
    }
}
