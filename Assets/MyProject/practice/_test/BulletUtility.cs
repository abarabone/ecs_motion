using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
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
    using Abarabone.Model;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    using CatId = PhysicsCategoryNamesId;
    using CatFlag = PhysicsCategoryNamesFlag;

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
                BelongsTo = CatFlag.datail,
                CollidesWith = CatFlag.bg_field | CatFlag.ch_detail,
            };
            var hitInput = new RaycastInput
            {
                Start = start,
                End = end,
                Filter = CollisionFilter.Default,//filter,
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

    }

}

