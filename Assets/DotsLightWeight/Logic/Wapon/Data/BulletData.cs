using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;

namespace DotsLite.Arms
{

    using DotsLite.Utilities;
    


    static public partial class Bullet
    {

        //public struct BeamTag : IComponentData
        //{ }

        //public struct SolidTag : IComponentData
        //{ }

        //public struct BulletData : IComponentData
        //{
        //    public Entity MainEnity;

        //    //public float3 Forward;
        //    public float RangeDistance;

        //    public float LifeTime;
        //    public float InvTotalTime;
        //}

        public struct LinkData : IComponentData
        {
            public Entity OwnerStateEntity;
        }

        public struct MoveSpecData : IComponentData
        {
            public float RangeDistanceFactor;
            public float BulletSpeed;
            //public float GravityFactor;
            public float AimFactor;
        }
        public struct PointDamageSpecData : IComponentData
        {
            public float Damage;
        }
        public struct SphereRangeDamageSpecData : IComponentData
        {
            public float Damage;
            public float Range;
        }
        public struct SubEmitData : IComponentData
        {
            public Entity EmitionEntity;
        }


        public struct HitResponceTypesData : IComponentData
        {
            public HitResponseTypes Types;
        }
        public enum HitResponseTypes
        {
            damage = 1 << 0,
            emit = 1 << 1,
            sticky = 1 << 2,

            no_destroy = 1 << 8,
        }

        //public struct VelocityData : IComponentData
        //{
        //    //public float3 Direction;
        //    //public DirectionAndLength DirAndLen;
        //    public float4 Velocity;
        //}
        //public struct AccelerationData : IComponentData
        //{
        //    //public DirectionAndLength DirAndLen;
        //    public float4 Acceleration;
        //}

        public struct DistanceData : IComponentData
        {
            public float RestRangeDistance;
        }

        public struct LifeTimeData : IComponentData
        {
            public float LifeTime;
            public float InvTotalTime;
        }
        // 残り時間は、できれば終了時刻に変更する
        // 残り距離は、時間に換算できないか検討する



        // function unit の emitting と同じ仕組みにできないかな…
        public struct EmitData : IComponentData
        {
            public Entity EmittingPrefab;
            public int numEmitting;
        }


        public struct InitializeFromEmitterData : IComponentData
        {
            public quaternion EmitterRotation;
            public float4 AimSpeed;
            public float EmitterAccuracyRad;
            public float EmitterRangeDistanceFactor;
        }
        //public struct Ini


        public struct RayTag : IComponentData
        { }

        public struct SphereTag : IComponentData
        { }

        public struct BeamTag : IComponentData
        { }

        public struct EmitterTag : IComponentData
        { }
    }


    public static class Explosion
    {
        public struct SpecData : IComponentData
        {
            public float HitRadius;
        }
    }

}
