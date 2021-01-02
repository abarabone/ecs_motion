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


    public struct ClosestHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }
        
        public Entity SelfMainEntity;
        public Entity TargetMainEntity;
        ComponentDataFromEntity<Bone.MainEntityLinkData> mainEntityLinks;
        
        T m_ClosestHit;
        public T ClosestHit => m_ClosestHit;

        public ClosestHitExcludeSelfCollector
            ( float maxFraction, Entity selfMainEntity, ComponentDataFromEntity<Bone.MainEntityLinkData> mainLinks )
        {
            this.MaxFraction = maxFraction;
            this.m_ClosestHit = default( T );
            this.SelfMainEntity = selfMainEntity;
            this.TargetMainEntity = Entity.Null;
            this.NumHits = 0;
            this.mainEntityLinks = mainLinks;
        }

        public bool AddHit( T hit )
        {
            var ent = this.mainEntityLinks.HasComponent(hit.Entity)
                ? this.mainEntityLinks[hit.Entity].MainEntity
                : hit.Entity;
            if( ent == this.SelfMainEntity ) return false;

            this.TargetMainEntity = ent;
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

        public Entity SelfMainEntity;
        public Entity TargetMainEntity;
        ComponentDataFromEntity<Bone.MainEntityLinkData> mainEntityLinks;

        public AnyHitExcludeSelfCollector
            (float maxFraction, Entity selfMainEntity, ComponentDataFromEntity<Bone.MainEntityLinkData> mainLinks)
        {
            this.MaxFraction = maxFraction;
            this.SelfMainEntity = selfMainEntity;
            this.TargetMainEntity = Entity.Null;
            this.NumHits = 0;
            this.mainEntityLinks = mainLinks;
        }

        public bool AddHit( T hit )
        {
            var ent = this.mainEntityLinks.HasComponent(hit.Entity)
                ? this.mainEntityLinks[hit.Entity].MainEntity
                : hit.Entity;
            if (ent == this.SelfMainEntity) return false;

            this.TargetMainEntity = ent;
            this.NumHits = 1;
            return true;
        }
    }
    
}
