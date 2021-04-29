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

namespace DotsLite.Model.Authoring
{

    [UpdateBefore(typeof(TrimBlankEntityFromLinkedEntityGroupConversion))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class LateDestroyEntityConversion : GameObjectConversionSystem
    {


        public struct TargetTag : IComponentData
        { }



        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            var desc0 = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(TargetTag),
                    typeof(Prefab),
                    typeof(Disabled)
                }
            };
            var desc1 = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(TargetTag),
                    typeof(Disabled)
                }
            };
            using var q = em.CreateEntityQuery(desc0, desc1);

            using var ents = q.ToEntityArray(Allocator.Temp);
            foreach (var ent in ents)
            {
                //Debug.Log(em.GetName_(ent));
                em.DestroyEntity(ent);
            }
        }

    }

    //[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    //[UpdateBefore(typeof(TrimBlankEntityFromLinkedEntityGroupConversion))]
    //public class LateDestroyEntityConversion : GameObjectConversionSystem
    //{


    //    public struct TargetTag : IComponentData
    //    { }



    //    protected override void OnUpdate()
    //    {
    //        var em = this.DstEntityManager;

    //        this.Entities
    //            .WithAll<TargetTag>()
    //            .ForEach
    //        (
    //            (Entity e) =>
    //            {
    //                Debug.Log(em.GetName_(e));
    //                em.DestroyEntity(e);
    //            }
    //        );
    //    }
    //}
}
