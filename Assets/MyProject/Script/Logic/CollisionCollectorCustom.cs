using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Abarabone.Physics
{

    
    public struct ClosestHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }
        
        Entity self;
        
        T m_ClosestHit;
        public T ClosestHit => m_ClosestHit;

        public ClosestHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity )
        {
            MaxFraction = maxFraction;
            m_ClosestHit = default( T );
            this.self = selfEntity;
            this.NumHits = 0;
        }

        public bool AddHit( T hit )
        {
            if( hit.Entity == this.self ) return false;
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

        public AnyHitExcludeSelfCollector( float maxFraction, Entity selfEntity )
        {
            this.MaxFraction = maxFraction;
            this.self = selfEntity;
            this.NumHits = 0;
        }

        public bool AddHit( T hit )
        {
            if( hit.Entity == this.self ) return false;
            this.NumHits = 1;
            return true;
        }
    }
    
}
