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


    public struct ClosestHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }
        public Hit.HitType HitType { get; private set; }
        
        public Entity SelfStateEntity;
        public Entity TargetStateEntity;
        ComponentDataFromEntity<Hit.TargetData> Targets;
        
        T m_ClosestHit;
        public T ClosestHit => m_ClosestHit;

        public ClosestHitExcludeSelfCollector
            ( float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets )
        {
            this.MaxFraction = maxFraction;
            this.m_ClosestHit = default;
            this.SelfStateEntity = selfStateEntity;
            this.TargetStateEntity = Entity.Null;
            this.NumHits = 0;
            this.Targets = targets;
            this.HitType = Hit.HitType.none;
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
                this.HitType = Hit.HitType.none;
                this.TargetStateEntity = hit.Entity;
            }

            //if( hit.Fraction >= m_ClosestHit.Fraction ) return false;
            this.MaxFraction = hit.Fraction;
            this.m_ClosestHit = hit;
            this.NumHits = 1;
            return true;
        }
    }
    



    public struct AnyHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => this.NumHits > 0;
        public float MaxFraction { get; }
        public int NumHits { get; private set; }
        public Hit.HitType HitType { get; private set; }

        public Entity SelfStateEntity;
        public Entity TargetStateEntity;
        ComponentDataFromEntity<Hit.TargetData> Targets;

        public AnyHitExcludeSelfCollector
            (float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets)
        {
            this.MaxFraction = maxFraction;
            this.SelfStateEntity = selfStateEntity;
            this.TargetStateEntity = Entity.Null;
            this.NumHits = 0;
            this.Targets = targets;
            this.HitType = Hit.HitType.none;
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
                this.HitType = Hit.HitType.none;
                this.TargetStateEntity = hit.Entity;
            }

            this.NumHits = 1;
            return true;
        }
    }
    
}
