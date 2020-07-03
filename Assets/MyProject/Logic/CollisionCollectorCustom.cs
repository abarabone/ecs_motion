using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;

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
        
        Entity self;
        ComponentDataFromEntity<BoneMainEntityLinkData> mainEntityLinks;
        
        T m_ClosestHit;
        public T ClosestHit => m_ClosestHit;

        public ClosestHitExcludeSelfCollector
            ( float maxFraction, Entity selfMainEntity, ComponentDataFromEntity<BoneMainEntityLinkData> mainLinks )
        {
            this.MaxFraction = maxFraction;
            this.m_ClosestHit = default( T );
            this.self = selfMainEntity;
            this.NumHits = 0;
            this.mainEntityLinks = mainLinks;
        }

        public bool AddHit( T hit )
        {
            var ent = this.mainEntityLinks.Exists(hit.Entity)
                ? this.mainEntityLinks[hit.Entity].MainEntity
                : hit.Entity;
            if( ent == this.self ) return false;

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

        Entity self;
        ComponentDataFromEntity<BoneMainEntityLinkData> mainEntityLinks;

        public AnyHitExcludeSelfCollector
            (float maxFraction, Entity selfMainEntity, ComponentDataFromEntity<BoneMainEntityLinkData> mainLinks)
        {
            this.MaxFraction = maxFraction;
            this.self = selfMainEntity;
            this.NumHits = 0;
            this.mainEntityLinks = mainLinks;
        }

        public bool AddHit( T hit )
        {
            var ent = this.mainEntityLinks.Exists(hit.Entity)
                ? this.mainEntityLinks[hit.Entity].MainEntity
                : hit.Entity;
            if (ent == this.self) return false;

            this.NumHits = 1;
            return true;
        }
    }
    
}
