using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;
using System;

namespace DotsLite.Physics
{

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using DotsLite.Model;
    using DotsLite.Collision;




    public struct ClosestJoinedHitExcludeSelfCollector<T> : ICollector<T>
        where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        public Targeting.Corps TargetCorps { get; private set; }
        public Entity TargetStateEntity { get; private set; }

        Targeting.Corps selfCorps;
        Entity selfStateEntity;
        ComponentDataFromEntity<Hit.TargetData> Targets;

        T m_ClosestHit;
        public T ClosestHit => m_ClosestHit;

        public ClosestJoinedHitExcludeSelfCollector(
            float maxFraction, Entity selfStateEntity, Targeting.Corps selfCorps,
            ComponentDataFromEntity<Hit.TargetData> targets)
        {
            this.MaxFraction = maxFraction;
            this.m_ClosestHit = default;
            this.NumHits = 0;

            this.TargetCorps = Targeting.Corps.none;
            this.TargetStateEntity = Entity.Null;

            this.selfCorps = selfCorps;
            this.selfStateEntity = selfStateEntity;

            this.Targets = targets;
        }

        public bool AddHit(T hit)
        {
            if (!this.Targets.HasComponent(hit.Entity)) return false;

            var t = this.Targets[hit.Entity];
            if (t.MainEntity == this.selfStateEntity || t.Corps == this.selfCorps) return false;

            this.TargetCorps = t.Corps;
            this.TargetStateEntity = t.MainEntity;

            this.MaxFraction = hit.Fraction;
            this.m_ClosestHit = hit;
            this.NumHits = 1;
            return true;
        }
    }



    public struct ClosestTargetedHitExcludeSelfCollector<T> : ICollector<T>
        where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }\

        public HitType TargetHitType { get; private set; }
        public Entity TargetStateEntity { get; private set; }

        Entity selfStateEntity;

        ComponentDataFromEntity<Hit.TargetData> Targets;
        
        T m_ClosestHit;
        public T ClosestHit => m_ClosestHit;

        public ClosestTargetedHitExcludeSelfCollector
            ( float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets )
        {
            this.MaxFraction = maxFraction;
            this.m_ClosestHit = default;
            this.NumHits = 0;

            this.TargetStateEntity = Entity.Null;
            this.TargetHitType = HitType.none;

            this.selfStateEntity = selfStateEntity;
            
            this.Targets = targets;
        }

        public bool AddHit( T hit )
        {
            if (this.Targets.HasComponent(hit.Entity))
            {
                var t = this.Targets[hit.Entity];
                if (t.MainEntity == this.selfStateEntity) return false;
                this.TargetHitType = t.HitType;
                this.TargetStateEntity = t.MainEntity;
            }
            else
            {
                if (hit.Entity == this.selfStateEntity) return false;
                this.TargetHitType = HitType.none;
                this.TargetStateEntity = hit.Entity;
            }

            //if( hit.Fraction >= m_ClosestHit.Fraction ) return false;
            this.MaxFraction = hit.Fraction;
            this.m_ClosestHit = hit;
            this.NumHits = 1;
            return true;
        }
    }
    



    public struct AnyTargetedHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => this.NumHits > 0;
        public float MaxFraction { get; }
        public int NumHits { get; private set; }
        public HitType HitType { get; private set; }

        public Entity SelfStateEntity;
        public Entity TargetStateEntity;
        ComponentDataFromEntity<Hit.TargetData> Targets;

        public AnyTargetedHitExcludeSelfCollector
            (float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets)
        {
            this.MaxFraction = maxFraction;
            this.SelfStateEntity = selfStateEntity;
            this.TargetStateEntity = Entity.Null;
            this.NumHits = 0;
            this.Targets = targets;
            this.HitType = HitType.none;
        }

        public bool AddHit( T hit )
        {
            if (this.Targets.HasComponent(hit.Entity))
            {
                var t = this.Targets[hit.Entity];
                if (t.MainEntity == this.SelfStateEntity) return false;
                this.HitType = t.HitType;
                this.TargetStateEntity = t.MainEntity;
            }
            else
            {
                if (hit.Entity == this.SelfStateEntity) return false;
                this.HitType = HitType.none;
                this.TargetStateEntity = hit.Entity;
            }

            this.NumHits = 1;
            return true;
        }
    }




    public struct AnyHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => this.NumHits > 0;
        public float MaxFraction { get; }
        public int NumHits { get; private set; }

        public Entity SelfEntity;
        public Entity OtherEntity;

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
