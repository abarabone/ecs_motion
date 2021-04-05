using System.Collections;
using System.Collections.Generic;
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


namespace Abarabone.Arms
{

    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Arms;
    using Abarabone.Character;
    using Abarabone.Draw;
    using Abarabone.Particle;
    using Abarabone.CharacterMotion;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Physics;
    using Abarabone.SystemGroup;
    using Abarabone.Structure;
    using Abarabone.Common;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    using CatId = PhysicsCategoryNamesId;
    using CatFlag = PhysicsCategoryNamesFlag;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.StructureHitMessage>;




    static public class Bulletss
    {

        static public void InstantiateBullet
            (
                this ref EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
                Entity bulletPrefab, float3 start, float3 end
            )
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
            public Entity mainEntity;
        }


        static public BulletHit BulletHitRay
            (
                ref this CollisionWorld cw,
                Entity selfMainEntity, float3 start, DirectionAndLength dir,
                ComponentDataFromEntity<Bone.MainEntityLinkData> links
            )
        {
            var end = start + dir.Direction * dir.Length;

            return cw.BulletHitRay(selfMainEntity, start, end, dir.Length, links);
        }

        static public BulletHit BulletHitRay
            (
                ref this CollisionWorld cw,
                Entity selfMainEntity, float3 start, float3 end, float distance,
                ComponentDataFromEntity<Bone.MainEntityLinkData> links
            )
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
            var collector = new ClosestHitExcludeSelfCollector<RaycastHit>(distance, selfMainEntity, links);
            var isHit = cw.CastRay(hitInput, ref collector);


            return new BulletHit
            {
                isHit = isHit,
                posision = collector.ClosestHit.Position,
                normal = collector.ClosestHit.SurfaceNormal,
                hitEntity = collector.ClosestHit.Entity,
                mainEntity = collector.TargetMainEntity,
            };
        }


        static public void postMessageToHitTarget
            (
                this BulletHitUtility.BulletHit hit,
                StructureHitHolder.ParallelWriter structureHitHolder,
                ComponentDataFromEntity<StructurePart.PartData> parts
            )
        {

            if (!hit.isHit) return;


            if (parts.HasComponent(hit.hitEntity))
            {
                structureHitHolder.Add(hit.mainEntity,
                    new StructureHitMessage
                    {
                        Position = hit.posision,
                        Normale = hit.normal,
                        PartEntity = hit.hitEntity,
                        PartId = parts[hit.hitEntity].PartId,
                    }
                );
            }
        }
        static public void postMessageToHitTarget<TJobInnerExecution>
            (
                this BulletHitUtility.BulletHit hit,
                HitMessageHolder<StructureHitMessage, TJobInnerExecution>.ParallelWriter hitHolder,
                ComponentDataFromEntity<StructurePart.PartData> parts
            )
            where TJobInnerExecution : struct, IHitMessageApplyJobExecution<StructureHitMessage>
        {

            if (!hit.isHit) return;


            if (parts.HasComponent(hit.hitEntity))
            {
                hitHolder.Add(hit.mainEntity,
                    new StructureHitMessage
                    {
                        Position = hit.posision,
                        Normale = hit.normal,
                        PartEntity = hit.hitEntity,
                        PartId = parts[hit.hitEntity].PartId,
                    }
                );
            }
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

