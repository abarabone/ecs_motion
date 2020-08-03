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

namespace Abarabone.Arms
{

    static public partial class Bullet
    {

        public struct BeamTag : IComponentData
        { }

        public struct SolidTag : IComponentData
        { }

        //public struct BulletData : IComponentData
        //{
        //    public Entity MainEnity;

        //    //public float3 Forward;
        //    public float RangeDistance;

        //    public float LifeTime;
        //    public float InvTotalTime;
        //}

        public struct Data : IComponentData
        {
            public Entity MainEntity;

            public bool IsCameraSight;
            public float RangeDistance;
            public float Speed;
        }
        public struct LifeTimeData : IComponentData
        {
            public float LifeTime;
            public float InvTotalTime;
        }
        public struct DirectionData : IComponentData
        {
            public float3 Direction;
        }

    }

}
