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
    using DotsLite.Targeting;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    using CatId = PhysicsCategoryNamesId;
    using CatFlag = PhysicsCategoryNamesFlag;

    using StructureHitHolder = NativeMultiHashMap<Entity, Structure.PartHitMessage>;


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



        // コマンドバッファで直接コンポーネントを渡す、でもいいのかも
        // → 加算とかする場合だめかも

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PostStructureEnvelopeHitMessage(
            this HitResultCore hit,
            HitMessage<Structure.EnvelopeHitMessage>.ParallelWriter hitHolder)
        {
            hitHolder.Add(hit.stateEntity,
                new Structure.EnvelopeHitMessage
                {

                }
            );
        }


        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PostStructurePartHitMessage(
            this HitResultCore hit,
            HitMessage<Structure.PartHitMessage>.ParallelWriter hitHolder,
            ComponentDataFromEntity<Part.PartData> parts)
        {
            if (!parts.HasComponent(hit.hitEntity)) return;

            hitHolder.Add(hit.stateEntity,
                new Structure.PartHitMessage
                {
                    Position = hit.posision,
                    Normal = hit.normal,
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void PostCharacterHitMessage(
            this HitResultCore hit,
            HitMessage<Character.HitMessage>.ParallelWriter hitHolder,
            float damage, float3 force,
            CorpsGroup.TargetWithArmsData selfcorps, CorpsGroup.Data othercorps)
        {
            if ((othercorps.BelongTo & selfcorps.TargetCorps) == 0) return;

            hit.PostCharacterHitMessage(hitHolder, damage, force);
        }



        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static public void DispatchHitMessage()
        //{


        //}



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Hit(this HitResultCore hit,
            HitMessage<Character.HitMessage>.ParallelWriter chhit,
            HitMessage<Structure.PartHitMessage>.ParallelWriter pthit,
            ComponentDataFromEntity<Part.PartData> parts,
            ComponentDataFromEntity<CorpsGroup.Data> corpss,
            float3 v, float damage,
            CorpsGroup.TargetWithArmsData corps)
        {
            switch (hit.hitType)
            {
                case HitType.part:

                    hit.PostStructurePartHitMessage(pthit, parts);
                    break;


                case HitType.charactor:

                    var otherCorpts = corpss[hit.hitEntity];
                    if ((otherCorpts.BelongTo & corps.TargetCorps) == 0) return;

                    hit.PostCharacterHitMessage(chhit, damage, v);
                    break;


                default:
                    break;
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public void Emit(this HitResultCore hit, 
            EntityCommandBuffer.ParallelWriter cmd, int eqi,
            Bullet.EmitData emit, Bullet.LinkData link, CorpsGroup.TargetWithArmsData corps)
        {
            var prefab = emit.EmittingPrefab;
            var state = link.OwnerStateEntity;
            var hpos = hit.posision;
            var cps = corps.TargetCorps;
            for (var i = 0; i < emit.numEmitting; i++)
            {
                emit_(cmd, eqi, prefab, state, hpos, cps);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void emit_(
            EntityCommandBuffer.ParallelWriter cmd, int eqi,
            Entity prefab, Entity stateEntity, float3 position, Corps targetCorps)
        {
            var instance = cmd.Instantiate(eqi, prefab);

            cmd.SetComponent(eqi, instance,
                new Translation
                {
                    Value = position,
                }
            );
            //cmd.SetComponent(eqi, instance,
            //    new Bullet.LinkData
            //    {
            //        OwnerStateEntity = stateEntity,
            //    }
            //);
            cmd.SetComponent(eqi, instance,
                new CorpsGroup.TargetWithArmsData
                {
                    TargetCorps = targetCorps,
                }
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sticky(this HitResultCore hit,
            EntityCommandBuffer.ParallelWriter cmd, int eqi, Entity self,
            Spring.StickyStateData state)
        {
            switch (f(state.NextSticky, hit.hitType))
            {
                case (int)Spring.NextStickyMode.first << 3 | (int)HitType.part:
                    //cmd.RemoveComponent<Spring.StickySelfFirstTag>(eqi, self);
                    //cmd.RemoveComponent<Particle.VelocitySpecData>(eqi, self);
                    ////cmd.AddComponent(eqi, self, new Spring.StickyTEntityFirstData
                    ////{
                    ////    Target = hit.hitEntity,
                    ////});
                    //cmd.AddComponent(eqi, self, new Spring.StickyPointFirstData
                    //{
                    //    Position = hit.posision.As_float4(),
                    //});
                    //cmd.SetComponent(eqi, self, new Spring.StickyStateData
                    //{
                    //    NextSticky = Spring.NextStickyMode.last,
                    //    PointLength = state.PointLength,
                    //});
                    //cmd.SetComponent(eqi, self, new Spring.StickyApplyData
                    //{
                    //    FirstFactor = 0.0f,
                    //    LastFactor = 1.0f,
                    //});
                    //cmd.AddComponent(eqi, self, new Spring.HittableSegmentData
                    //{
                    //    Index = new int2(state.PointLength-2, state.PointLength-1),
                    //});
                    ////cmd.SetComponent(eqi, self, new Particle.VelocityFactorData
                    ////{
                    ////    PrePosition = 
                    ////});
                    break;

                case (int)Spring.NextStickyMode.first << 3 | (int)HitType.charactor:
                    //cmd.RemoveComponent<Spring.StickySelfFirstTag>(eqi, self);
                    //cmd.RemoveComponent<Particle.VelocitySpecData>(eqi, self);
                    ////cmd.AddComponent(eqi, self, new Spring.StickyTREntityFirstData
                    ////{
                    ////    Target = hit.hitEntity,
                    ////    LocalPosition = ,
                    ////});
                    //cmd.AddComponent(eqi, self, new Spring.StickyPointFirstData
                    //{
                    //    Position = hit.posision.As_float4(),
                    //});
                    //cmd.SetComponent(eqi, self, new Spring.StickyStateData
                    //{
                    //    NextSticky = Spring.NextStickyMode.last,
                    //    PointLength = state.PointLength,
                    //});
                    //cmd.SetComponent(eqi, self, new Spring.StickyApplyData
                    //{
                    //    FirstFactor = 0.0f,
                    //    LastFactor = 1.0f,
                    //});
                    //cmd.AddComponent(eqi, self, new Spring.HittableSegmentData
                    //{
                    //    Index = new int2(state.PointLength - 2, state.PointLength - 1),
                    //});
                    ////cmd.SetComponent(eqi, self, new Particle.VelocityFactorData
                    ////{
                    ////    PrePosition = 
                    ////});
                    break;

                case (int)Spring.NextStickyMode.first << 3 | (int)HitType.none:
                    cmd.RemoveComponent<Spring.StickySelfFirstTag>(eqi, self);
                    cmd.RemoveComponent<Particle.VelocitySpecData>(eqi, self);
                    //cmd.AddComponent(eqi, self, new Spring.StickyTEntityFirstData
                    //{
                    //    Target = hit.hitEntity,
                    //});
                    cmd.AddComponent(eqi, self, new Spring.StickyPointFirstData
                    {
                        Position = hit.posision.As_float4(),
                    });
                    cmd.SetComponent(eqi, self, new Spring.StickyStateData
                    {
                        NextSticky = Spring.NextStickyMode.last,
                        PointLength = state.PointLength,
                    });
                    cmd.SetComponent(eqi, self, new Spring.StickyApplyData
                    {
                        FirstFactor = 0.0f,
                        LastFactor = 1.0f,
                    });
                    cmd.AddComponent(eqi, self, new Spring.HittableSegmentData
                    {
                        Index = new int2(state.PointLength - 2, state.PointLength - 1),
                    });
                    //cmd.SetComponent(eqi, self, new Particle.VelocityFactorData
                    //{
                    //    PrePosition = 
                    //});
                    break;


                case (int)Spring.NextStickyMode.last << 3 | (int)HitType.part:
                    //cmd.AddComponent(eqi, self, new Spring.StickyPointLastData
                    //{
                    //    Position = hit.posision.As_float4(),
                    //});
                    //cmd.SetComponent(eqi, self, new Spring.StickyStateData
                    //{
                    //    NextSticky = Spring.NextStickyMode.completed,
                    //    PointLength = state.PointLength,
                    //});
                    //cmd.SetComponent(eqi, self, new Spring.StickyApplyData
                    //{
                    //    FirstFactor = 0.0f,
                    //    LastFactor = 0.0f,
                    //});
                    //cmd.SetComponent(eqi, self, new Spring.HittableSegmentData
                    //{
                    //    Index = new int2(0, 1),
                    //});
                    //cmd.RemoveComponent<Translation>(eqi, self);
                    //cmd.RemoveComponent<Psyllium.TranslationTailData>(eqi, self);
                    break;

                case (int)Spring.NextStickyMode.last << 3 | (int)HitType.charactor:

                    break;

                case (int)Spring.NextStickyMode.last << 3 | (int)HitType.none:
                    cmd.AddComponent(eqi, self, new Spring.StickyPointLastData
                    {
                        Position = hit.posision.As_float4(),
                    });
                    cmd.SetComponent(eqi, self, new Spring.StickyStateData
                    {
                        NextSticky = Spring.NextStickyMode.completed,
                        PointLength = state.PointLength,
                    });
                    cmd.SetComponent(eqi, self, new Spring.StickyApplyData
                    {
                        FirstFactor = 0.0f,
                        LastFactor = 0.0f,
                    });
                    cmd.SetComponent(eqi, self, new Spring.HittableSegmentData
                    {
                        Index = new int2(0, 1),
                    });
                    cmd.RemoveComponent<Translation>(eqi, self);
                    cmd.RemoveComponent<Psyllium.TranslationTailData>(eqi, self);
                    break;

                default:
                    break;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int f(Spring.NextStickyMode state, HitType hittype) => (int)state << 3 | (int)hittype;
        // これリテラルに使う方法ないのか？
    }


}

