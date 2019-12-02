using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;
using Abss.Character;
using Abss.Motion;

namespace Abss.Character
{



    //[DisableAutoCreation]
    [UpdateInGroup( typeof( ObjectLogicSystemGroup ) )]
    public class AntMoveDirectionSystem : JobComponentSystem
    {



        protected override void OnCreate()
        { }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new AiJob
            { }
            .Schedule( this, inputDeps );

            return inputDeps;
        }



        [BurstCompile, RequireComponentTag(typeof(AntTag))]
        struct AiJob : IJobForEachWithEntity
            <MoveHandlingData, Rotation>//
        {


            public void Execute(
                Entity entity, int index,
                [WriteOnly] ref MoveHandlingData handler,
                [ReadOnly] ref Rotation rot
            )
            {

                //handler.ControlAction.HorizontalRotation = quaternion.identity;
                //handler.ControlAction.LookRotation = quaternion.identity;
                handler.ControlAction.MoveDirection = math.forward(rot.Value);

            }
        }




        //public struct ClosestHitExcludeSelfCollector<T> : ICollector<T> where T : struct, IQueryResult
        public struct ClosestHitExcludeSelfCollector : ICollector<RaycastHit>
        {
            public bool EarlyOutOnFirstHit => false;
            public float MaxFraction { get; private set; }
            public int NumHits { get; private set; }

            private RaycastHit m_ClosestHit;
            public RaycastHit ClosestHit => m_ClosestHit;

            NativeSlice<RigidBody> rigidbodies;
            Entity self;

            public ClosestHitExcludeSelfCollector
                ( float maxFraction, Entity selfEntity, NativeSlice<RigidBody> rigidbodies )
            {
                MaxFraction = maxFraction;
                m_ClosestHit = default( RaycastHit );
                NumHits = 0;
                this.rigidbodies = rigidbodies;
                this.self = selfEntity;
            }


            public bool AddHit( RaycastHit hit )
            {
                if( this.rigidbodies[ hit.RigidBodyIndex ].Entity == this.self ) return false;

                MaxFraction = hit.Fraction;
                m_ClosestHit = hit;
                NumHits = 1;
                return true;
            }

            public void TransformNewHits
                ( int oldNumHits, float oldFraction, Unity.Physics.Math.MTransform transform, uint numSubKeyBits, uint subKey )
            {
                if( m_ClosestHit.Fraction < oldFraction )
                {
                    m_ClosestHit.Transform( transform, numSubKeyBits, subKey );
                }
            }

            public void TransformNewHits
                ( int oldNumHits, float oldFraction, Unity.Physics.Math.MTransform transform, int rigidBodyIndex )
            {
                if( m_ClosestHit.Fraction < oldFraction )
                {
                    m_ClosestHit.Transform( transform, rigidBodyIndex );
                }
            }
        }
    }

}
