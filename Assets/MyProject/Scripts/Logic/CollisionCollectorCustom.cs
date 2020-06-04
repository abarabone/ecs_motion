using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Physics;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Abss.Physics
{


    public struct ClosestRayHitExcludeSelfCollector : ICollector<RaycastHit>
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        NativeSlice<RigidBody> rigidbodies;
        Entity self;

        //RaycastHit currentHit;
        RaycastHit m_ClosestHit;
        public RaycastHit ClosestHit => m_ClosestHit;

        public ClosestRayHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            m_ClosestHit = default( RaycastHit );
            //this.currentHit = default( RaycastHit );
            this.rigidbodies = rigidbodies;
            this.self = selfEntity;
            this.NumHits = 0;
        }

        public bool AddHit( RaycastHit hit )
        {
            if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return false;
            this.MaxFraction = hit.Fraction;
            this.m_ClosestHit = hit;
            this.NumHits = 1;
            //this.currentHit = hit;
            return true;
        }

        //public void TransformNewHits
        //    ( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey )
        //{
        //    //if( m_ClosestHit.Fraction < oldFraction )
        //    //{
        //    //    m_ClosestHit.Transform( transform, numSubKeyBits, subKey );
        //    //}
        //}
        //public void TransformNewHits
        //    ( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
        //{
        //    //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
        //    if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;

        //    if( this.currentHit.Fraction < oldFraction )
        //    {
        //        m_ClosestHit = this.currentHit;
        //        m_ClosestHit.Transform( transform, rigidBodyIndex );
        //        MaxFraction = m_ClosestHit.Fraction;
        //        NumHits = 1;
        //    }
        //}
    }

    public struct ClosestDistanceHitExcludeSelfCollector : ICollector<DistanceHit>
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        NativeSlice<RigidBody> rigidbodies;
        Entity self;

        //DistanceHit currentHit;
        DistanceHit m_ClosestHit;
        public DistanceHit ClosestHit => m_ClosestHit;

        public ClosestDistanceHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            m_ClosestHit = default( DistanceHit );
            //this.currentHit = default( DistanceHit );
            this.rigidbodies = rigidbodies;
            this.self = selfEntity;
            this.NumHits = 0;
        }

        public bool AddHit( DistanceHit hit )
        {
            if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return false;
            this.MaxFraction = hit.Fraction;
            this.m_ClosestHit = hit;
            this.NumHits = 1;
            return true;
        }

        //public void TransformNewHits
        //    ( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey )
        //{
        //    //if( m_ClosestHit.Fraction < oldFraction )
        //    //{
        //    //    m_ClosestHit.Transform( transform, numSubKeyBits, subKey );
        //    //}
        //}
        //public void TransformNewHits
        //    ( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
        //{
        //    //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
        //    if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;

        //    if( this.currentHit.Fraction < oldFraction )
        //    {
        //        m_ClosestHit = this.currentHit;
        //        m_ClosestHit.Transform( transform, rigidBodyIndex );
        //        MaxFraction = m_ClosestHit.Fraction;
        //        NumHits = 1;
        //    }
        //}
    }



    public struct AnyDistanceHitExcludeSelfCollector : ICollector<DistanceHit>
    {
        public bool EarlyOutOnFirstHit => false;//{ get; private set; }
        public float MaxFraction { get; private set; }
        public int NumHits => 0;//{ get; private set; }

        NativeSlice<RigidBody> rigidbodies;
        Entity self;

        public AnyDistanceHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            this.rigidbodies = rigidbodies;
            this.self = selfEntity;
            //this.NumHits = 0;
        }

        public bool AddHit( DistanceHit hit )
        {
            if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return false;
            this.MaxFraction = hit.Fraction;
            //this.NumHits++;
            return true;
        }

        //public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey ) { }
        //public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
        //{
        //    //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
        //    if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;
        //    this.NumHits++;
        //}
    }

    public struct AnyRayHitExcludeSelfCollector : ICollector<RaycastHit>
    {
        public bool EarlyOutOnFirstHit => true;//{ get; private set; }
        public float MaxFraction { get; }//; private set; }
        public int NumHits => 0;//{ get; private set; }

        NativeSlice<RigidBody> rigidbodies;
        Entity self;

        public AnyRayHitExcludeSelfCollector
            ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
        {
            MaxFraction = maxFraction;
            this.rigidbodies = rigidbodies;
            this.self = selfEntity;
            //this.NumHits = 0;
        }

        public bool AddHit( RaycastHit hit )
        {
            if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return false;
            //this.MaxFraction = hit.Fraction;
            //this.NumHits++;
            return true;
        }

        //public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, uint numSubKeyBits, uint subKey ) { }
        //public void TransformNewHits( int oldNumHits, float oldFraction, Math.MTransform transform, int rigidBodyIndex )
        //{
        //    //Debug.Log( $"{rigidBodyIndex} {this.rigidbodies[ rigidBodyIndex ].Entity}" );
        //    if( this.rigidbodies[ rigidBodyIndex ].Entity == this.self ) return;
        //    this.NumHits++;
        //}
    }
}
