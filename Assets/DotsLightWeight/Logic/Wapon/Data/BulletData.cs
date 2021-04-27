﻿using System.Collections;
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

namespace Abarabone.Arms
{

    using Abarabone.Utilities;


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

        public struct SpecData : IComponentData
        {
            //public Entity MainEntity;
            public Entity StateEntity;

            public float RangeDistanceFactor;
            public float BulletSpeed;
            public float GravityFactor;
            public float AimFactor;
        }

        public struct VelocityData : IComponentData
        {
            //public float3 Direction;
            //public DirectionAndLength DirAndLen;
            public float4 Velocity;
        }
        public struct AccelerationData : IComponentData
        {
            //public DirectionAndLength DirAndLen;
            public float4 Acceleration;
        }

        public struct DistanceData : IComponentData
        {
            public float RestRangeDistance;
        }

        public struct LifeTimeData : IComponentData
        {
            public float LifeTime;
            public float InvTotalTime;
        }

    }

}
