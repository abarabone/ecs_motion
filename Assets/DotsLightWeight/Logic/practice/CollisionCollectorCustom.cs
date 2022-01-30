using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;
using System;
using Unity.Mathematics;

namespace DotsLite.Collision
{

    using DotsLite.Character;
    using DotsLite.Collision;
    using DotsLite.Targeting;


    public static class CollectorExtension
    {
        public static ClosestCorpsExcludeSelfCollector<T> GetClosestCollector<T>(
            this ComponentDataFromEntity<CorpsGroup.Data> corpss, float maxFraction, Corps targetCorps)
            where T : struct, IQueryResult
        =>
            new ClosestCorpsExcludeSelfCollector<T>(maxFraction, targetCorps, corpss);


        public static ClosestTargetedHitExcludeSelfCollector<T> GetClosestCollector<T>(
            this ComponentDataFromEntity<Hit.TargetData> targets, float maxFraction, Entity selfStateEntity)
            where T : struct, IQueryResult
        =>
            new ClosestTargetedHitExcludeSelfCollector<T>(maxFraction, selfStateEntity, targets);


        public static AnyTargetedHitExcludeSelfCollector<T> GetAnyCollector<T>(
            this ComponentDataFromEntity<Hit.TargetData> targets, float maxFraction, Entity selfStateEntity)
            where T : struct, IQueryResult
        =>
            new AnyTargetedHitExcludeSelfCollector<T>(maxFraction, selfStateEntity, targets);



        public static AllTargetedDistanceHitCollector GetAllDistanceCollector(
            this ComponentDataFromEntity<Hit.TargetData> targets,
            float maxFraction, NativeList<DistanceHitResult> outputs)
        =>
            new AllTargetedDistanceHitCollector(maxFraction, targets, outputs);
    }



    public struct HitResultCore
    {
        public HitType hitType;
        public float3 posision;
        public float3 normal;
        public Entity hitEntity;
        public Entity stateEntity;
        public uint childId;
    }

    public struct DistanceHitResult
    {
        public HitResultCore core;
        public float distance;
    }

    /// <summary>
    /// type 取得、state entity 取得、すべて
    /// </summary>
    public struct AllTargetedDistanceHitCollector : ICollector<DistanceHit>
    {
        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }


        ComponentDataFromEntity<Hit.TargetData> targets;

        NativeList<DistanceHitResult> hitResults;


        public AllTargetedDistanceHitCollector
            (float maxFraction, ComponentDataFromEntity<Hit.TargetData> targets, NativeList<DistanceHitResult> outputs)
        {
            this.hitResults = outputs;

            this.MaxFraction = maxFraction;
            this.NumHits = 0;

            this.targets = targets;
        }

        public bool AddHit(DistanceHit hit)
        {
            var res = new DistanceHitResult
            {
                core = new HitResultCore
                {
                    hitEntity = hit.Entity,
                    posision = hit.Position,
                    normal = hit.SurfaceNormal,
                    childId = hit.ColliderKey.Value,
                },
                distance = hit.Distance,
            };

            if (this.targets.HasComponent(hit.Entity))
            {
                var t = this.targets[hit.Entity];
                res.core.hitType = t.HitType;
                res.core.stateEntity = t.MainEntity;
            }
            else
            {
                res.core.hitType = HitType.none;
                res.core.stateEntity = hit.Entity;
            }

            this.hitResults.Add(res);
            this.NumHits = this.hitResults.Length;
            return true;
        }
    }
    //public struct AllTargetedHitExcludeSelfCollector<T> : ICollector<T>//, IDisposable
    //    where T : struct, IQueryResult
    //{
    //    public bool EarlyOutOnFirstHit => false;
    //    public float MaxFraction { get; private set; }
    //    public int NumHits { get; private set; }


    //    Entity selfStateEntity;

    //    ComponentDataFromEntity<Hit.TargetData> targets;

    //    NativeList<HitResult> hitResults;


    //    public AllTargetedHitExcludeSelfCollector
    //        (float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets, NativeList<HitResult> outputs)
    //    {
    //        this.hitResults = outputs;

    //        this.MaxFraction = maxFraction;
    //        this.NumHits = 0;

    //        this.selfStateEntity = selfStateEntity;
    //        this.targets = targets;
    //    }

    //    public bool AddHit(T hit)
    //    {
    //        var res = new HitResult
    //        {
    //            hitEntity = hit.Entity,
    //        };

    //        if (this.targets.HasComponent(hit.Entity))
    //        {
    //            var t = this.targets[hit.Entity];
    //            if (t.MainEntity == this.selfStateEntity) return false;

    //            res.hitType = t.HitType;
    //            res.stateEntity = t.MainEntity;
    //        }
    //        else
    //        {
    //            if (hit.Entity == this.selfStateEntity) return false;

    //            res.hitType = HitType.none;
    //            res.stateEntity = hit.Entity;
    //        }

    //        this.hitResults.Add(res);
    //        this.NumHits = this.hitResults.Length;
    //        return true;
    //    }
    //}




    /// <summary>
    /// 軍指定、自己除外、再近傍
    /// CorpsGroup.Data を持つ Entity は、必ず state entity
    /// </summary>
    public struct ClosestCorpsExcludeSelfCollector<T> : ICollector<T>
        where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        public Corps OtherCorps { get; private set; }
        public Corps TargetCorps { get; private set; }

        public T ClosestHit { get; private set; }

        ComponentDataFromEntity<CorpsGroup.Data> corpss;


        public ClosestCorpsExcludeSelfCollector(
            float maxFraction, Corps targetCorps, ComponentDataFromEntity<CorpsGroup.Data> corpss)
        {
            this.MaxFraction = maxFraction;
            this.ClosestHit = default;
            this.NumHits = 0;
            this.OtherCorps = Corps.none;

            this.TargetCorps = targetCorps;

            this.corpss = corpss;
        }

        public bool AddHit(T hit)
        {
            if (!this.corpss.HasComponent(hit.Entity)) return false;

            var t = this.corpss[hit.Entity];
            if ((t.BelongTo & this.TargetCorps) == 0) return false;

            this.OtherCorps = t.BelongTo;

            this.MaxFraction = hit.Fraction;
            this.ClosestHit = hit;
            this.NumHits = 1;
            return true;
        }
    }



    /// <summary>
    /// type 取得、state entity 取得、自己除外、再近傍
    /// </summary>
    public struct ClosestTargetedHitExcludeSelfCollector<T> : ICollector<T>
        where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        public HitType OtherHitType { get; private set; }
        public Entity OtherStateEntity { get; private set; }

        Entity selfStateEntity;

        ComponentDataFromEntity<Hit.TargetData> targets;
        
        public T ClosestHit { get; private set; }

        public ClosestTargetedHitExcludeSelfCollector
            ( float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets )
        {
            this.MaxFraction = maxFraction;
            this.ClosestHit = default;
            this.NumHits = 0;

            this.OtherStateEntity = Entity.Null;
            this.OtherHitType = HitType.none;

            this.selfStateEntity = selfStateEntity;
            
            this.targets = targets;
        }

        public bool AddHit( T hit )
        {
            if (this.targets.HasComponent(hit.Entity))
            {
                var t = this.targets[hit.Entity];
                if (t.MainEntity == this.selfStateEntity) return false;
                this.OtherHitType = t.HitType;
                this.OtherStateEntity = t.MainEntity;
            }
            else
            {
                if (hit.Entity == this.selfStateEntity) return false;
                this.OtherHitType = HitType.none;
                this.OtherStateEntity = hit.Entity;
            }

            //if( hit.Fraction >= m_ClosestHit.Fraction ) return false;
            this.MaxFraction = hit.Fraction;
            this.ClosestHit = hit;
            this.NumHits = 1;
            return true;
        }
    }




    /// <summary>
    /// 種類取得、state entity 取得、自己除外、１つでも
    /// </summary>
    public struct AnyTargetedHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => this.NumHits > 0;
        public float MaxFraction { get; }
        public int NumHits { get; private set; }
        public HitType OtherHitType { get; private set; }

        public Entity SelfStateEntity { get; private set; }
        public Entity OtherStateEntity { get; private set; }
        ComponentDataFromEntity<Hit.TargetData> targets;

        public AnyTargetedHitExcludeSelfCollector
            (float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets)
        {
            this.MaxFraction = maxFraction;
            this.NumHits = 0;
            this.OtherHitType = HitType.none;
            this.OtherStateEntity = Entity.Null;

            this.SelfStateEntity = selfStateEntity;
            this.targets = targets;
        }

        public bool AddHit( T hit )
        {
            if (this.targets.HasComponent(hit.Entity))
            {
                var t = this.targets[hit.Entity];
                if (t.MainEntity == this.SelfStateEntity) return false;
                this.OtherHitType = t.HitType;
                this.OtherStateEntity = t.MainEntity;
            }
            else
            {
                if (hit.Entity == this.SelfStateEntity) return false;
                this.OtherHitType = HitType.none;
                this.OtherStateEntity = hit.Entity;
            }

            this.NumHits = 1;
            return true;
        }
    }




    /// <summary>
    /// 自己除外、１つでも
    /// </summary>
    public struct AnyHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => this.NumHits > 0;
        public float MaxFraction { get; }
        public int NumHits { get; private set; }

        public Entity SelfEntity { get; private set; }
        public Entity OtherEntity { get; private set; }

        public AnyHitExcludeSelfCollector(float maxFraction, Entity selfStateEntity)
        {
            this.MaxFraction = maxFraction;
            this.SelfEntity = selfStateEntity;
            this.OtherEntity = Entity.Null;
            this.NumHits = 0;
        }

        public bool AddHit(T hit)
        {
            if (hit.Entity == this.SelfEntity) return false;

            this.OtherEntity = hit.Entity;
            this.NumHits = 1;
            return true;
        }
    }

}
