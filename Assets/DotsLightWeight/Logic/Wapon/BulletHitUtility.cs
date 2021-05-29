using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Arms
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Draw;
    using DotsLite.Particle;
    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Collision;
    using DotsLite.SystemGroup;
    using DotsLite.Structure;
    using DotsLite.Dependency;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    using CatId = PhysicsCategoryNamesId;
    using CatFlag = PhysicsCategoryNamesFlag;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.HitMessage>;


    static public class BulletHitUtility
    {

        public struct BulletHit
        {
            public bool isHit;
            public HitResultCore core;
        }



        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BulletHit BulletHitRay(
            ref this CollisionWorld cw,
            Entity selfStateEntity, float3 start, DirectionAndLength dir,
            ComponentDataFromEntity<Hit.TargetData> targets)
        {
            var end = start + dir.Direction * dir.Length;

            return cw.BulletHitRay(selfStateEntity, start, end, dir.Length, targets);
        }



        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BulletHit BulletHitRay(
            ref this CollisionWorld cw,
            Entity selfStateEntity, float3 start, float3 end, float distance,
            ComponentDataFromEntity<Hit.TargetData> targets)
        {

            var filter = new CollisionFilter
            {
                BelongsTo = CollisionFilter.Default.BelongsTo,
                CollidesWith = CatFlag.datail | CatFlag.field | CatFlag.detenv,
            };

            var hitInput = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = filter,//CollisionFilter.Default,//
            };

            var collector = new ClosestTargetedHitExcludeSelfCollector<RaycastHit>(distance, selfStateEntity, targets);

            var isHit = cw.CastRay(hitInput, ref collector);


            return new BulletHit
            {
                isHit = isHit,
                core = new HitResultCore
                {
                    hitType = collector.OtherHitType,
                    posision = collector.ClosestHit.Position,
                    normal = collector.ClosestHit.SurfaceNormal,
                    hitEntity = collector.ClosestHit.Entity,
                    stateEntity = collector.OtherStateEntity,
                }
            };
        }


        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BulletHit BulletHitSphere(
            ref this CollisionWorld cw,
            Entity selfStateEntity, float3 pos, float radius,
            ComponentDataFromEntity<Hit.TargetData> links)
        {

            var filter = new CollisionFilter
            {
                BelongsTo = CollisionFilter.Default.BelongsTo,
                CollidesWith = CatFlag.datail | CatFlag.field | CatFlag.detenv,
            };

            var collector = new ClosestTargetedHitExcludeSelfCollector<DistanceHit>(radius, selfStateEntity, links);

            var isHit = cw.OverlapSphereCustom(pos, radius, ref collector, filter);


            return new BulletHit
            {
                isHit = isHit,
                core = new HitResultCore
                {
                    hitType = collector.OtherHitType,
                    posision = collector.ClosestHit.Position,
                    normal = collector.ClosestHit.SurfaceNormal,
                    hitEntity = collector.ClosestHit.Entity,
                    stateEntity = collector.OtherStateEntity,
                }
            };
        }



        //static public void postMessageToHitTarget
        //    (
        //        this BulletHitUtility.BulletHit hit,
        //        StructureHitHolder.ParallelWriter structureHitHolder,
        //        ComponentDataFromEntity<StructurePart.PartData> parts
        //    )
        //{

        //    if (!hit.isHit) return;


        //    if (parts.HasComponent(hit.hitEntity))
        //    {
        //        structureHitHolder.Add(hit.mainEntity,
        //            new StructureHitMessage
        //            {
        //                Position = hit.posision,
        //                Normale = hit.normal,
        //                PartEntity = hit.hitEntity,
        //                PartId = parts[hit.hitEntity].PartId,
        //            }
        //        );
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PostStructureHitMessage(
            this HitResultCore hit,
            HitMessage<Structure.HitMessage>.ParallelWriter hitHolder,
            ComponentDataFromEntity<StructurePart.PartData> parts)
        {
            if (!parts.HasComponent(hit.hitEntity)) return;

            hitHolder.Add(hit.stateEntity,
                new Structure.HitMessage
                {
                    Position = hit.posision,
                    Normale = hit.normal,
                    PartEntity = hit.hitEntity,
                    PartId = parts[hit.hitEntity].PartId,
                    //Force = ,
                }
            );
        }
        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PostCharacterHitMessage(
            this HitResultCore hit,
            HitMessage<Character.HitMessage>.ParallelWriter hitHolder,
            float damage, float3 force)
        {
            hitHolder.Add(hit.stateEntity,
                new Character.HitMessage
                {
                    Position = hit.posision,
                    Normale = hit.normal,
                    Damage = damage,
                    Force = force,
                }
            );
        }

    }


}

