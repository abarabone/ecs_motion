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
    using DotsLite.Physics;
    using DotsLite.SystemGroup;
    using DotsLite.Structure;
    using DotsLite.Dependency;
    using DotsLite.Hit;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    using CatId = PhysicsCategoryNamesId;
    using CatFlag = PhysicsCategoryNamesFlag;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.HitMessage>;




    static public class Bulletss
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void InstantiateBullet(
            this ref EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            Entity bulletPrefab, float3 start, float3 end)
        {
            var newBeamEntity = cmd.Instantiate(uniqueIndex, bulletPrefab);

            cmd.SetComponent(uniqueIndex, newBeamEntity,
                new Particle.TranslationPtoPData
                {
                    Start = start,
                    End = end,
                }
            );
        }

    }





    static public class BulletHitUtility
    {

        public struct BulletHit
        {
            public bool isHit;
            public float3 posision;
            public float3 normal;
            public Entity hitEntity;
            public Entity stateEntity;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BulletHit BulletHitRay(
            ref this CollisionWorld cw,
            Entity selfStateEntity, float3 start, DirectionAndLength dir,
            ComponentDataFromEntity<Hit.TargetData> links)
        {
            var end = start + dir.Direction * dir.Length;

            return cw.BulletHitRay(selfStateEntity, start, end, dir.Length, links);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public BulletHit BulletHitRay(
            ref this CollisionWorld cw,
            Entity selfStateEntity, float3 start, float3 end, float distance,
            ComponentDataFromEntity<Hit.TargetData> links)
        {

            var filter = new CollisionFilter
            {
                BelongsTo = CollisionFilter.Default.BelongsTo,
                CollidesWith = CatFlag.datail | CatFlag.field,// | CatFlag.detenv,
            };

            var hitInput = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = filter,//CollisionFilter.Default,//
            };

            var collector = new ClosestHitExcludeSelfCollector<RaycastHit>(distance, selfStateEntity, links);

            var isHit = cw.CastRay(hitInput, ref collector);


            return new BulletHit
            {
                isHit = isHit,
                posision = collector.ClosestHit.Position,
                normal = collector.ClosestHit.SurfaceNormal,
                hitEntity = collector.ClosestHit.Entity,
                stateEntity = collector.TargetStateEntity,
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PostStructureHitMessage(
            this BulletHit hit,
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
                }
            );
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PostCharacterHitMessage(
            this BulletHit hit,
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


    static public class ColorExtension
    {
        static public int4 to_int4(this Color32 color) => new int4(color.r, color.g, color.b, color.a);
        static public Color32 ToColor32(this int4 color) => new Color32((byte)color.x, (byte)color.y, (byte)color.z, (byte)color.w);

        static public float4 to_float4(this Color32 color) => new float4(color.r, color.g, color.b, color.a);
        static public Color32 ToColor32(this float4 color) => new Color32((byte)color.x, (byte)color.y, (byte)color.z, (byte)color.w);

        static public Color32 ApplyAlpha(this Color32 color, float newAlpha)
        {
            color.a = (byte)(newAlpha * 255);
            return color;
        }
    }
}

