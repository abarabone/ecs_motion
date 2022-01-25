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

namespace DotsLite.EntityTrimmer.Authoring
{

    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateBefore(typeof(RemoveTransformAllConversion))]
    [UpdateBefore(typeof(DestroyBlankEntityConversion))]
    public class DestroyEntityLateConversion : GameObjectConversionSystem
    {


        public struct TargetTag : IComponentData
        { }



        protected override void OnUpdate()
        {
        //}
        //protected override void OnDestroy()
        //{
            var em = this.DstEntityManager;

            var desc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(TargetTag),
                },
                Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled,
            };
            using var q = em.CreateEntityQuery(desc);
            em.DestroyEntity(q);

            //using var ents = q.ToEntityArray(Allocator.Temp);
            //foreach (var ent in ents)
            //{
            //    //Debug.Log(em.GetName_(ent));
            //    em.DestroyEntity(ent);
            //}
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
