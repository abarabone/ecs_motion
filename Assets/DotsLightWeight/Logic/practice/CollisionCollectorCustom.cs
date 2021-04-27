using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;
using System;

namespace Abarabone.Physics
{

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Abarabone.Model;
    using Abarabone.Hit;


    public struct ClosestHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }
        
        public Entity SelfStateEntity;
        public Entity TargetStateEntity;
        ComponentDataFromEntity<Hit.TargetData> Targets;
        
        T m_ClosestHit;
        public T ClosestHit => m_ClosestHit;

        public ClosestHitExcludeSelfCollector
            ( float maxFraction, Entity selfStateEntity, ComponentDataFromEntity<Hit.TargetData> targets )
        {
            this.MaxFraction = maxFraction;
            this.m_ClosestHit = default( T );
            this.SelfStateEntity = selfStateEntity;
            this.TargetStateEntity = Entity.Null;
            this.NumHits = 0;
            this.Targets = targets;
        }

        public bool AddHit( T hit )
        {
            var ent = this.Targets.HasComponent(hit.Entity)
                ? this.Targets[hit.Entity].StateEntity
                : hit.Entity;
            if( ent == this.SelfStateEntity ) return false;

            this.TargetStateEntity = ent;
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
        }

        public bool AddHit( T hit )
        {
            var ent = this.Targets.HasComponent(hit.Entity)
                ? this.Targets[hit.Entity].StateEntity
                : hit.Entity;
            if (ent == this.SelfStateEntity) return false;

            this.TargetStateEntity = ent;
            this.NumHits = 1;
            return true;
        }
    }
    
}
