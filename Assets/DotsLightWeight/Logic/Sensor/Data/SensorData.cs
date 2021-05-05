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
using Unity.Physics.Authoring;

namespace DotsLite.Targeting
{

    public enum Corps
    {
        none = 0,
        human = 1 << 0,
        invader = 1 << 1,
        forener = 1 << 2,
        primer = 1 << 3,
        chronus = 1 << 4,
        core = 1 << 5,
        abyss = 1 << 6,
        triffids = 1 << 7,
        predator = 1 << 8,
        messenger = 1 << 9,
    }


    static public partial class CorpsGroup
    {
        public struct TargetWithArmsData : IComponentData
        {
            public Corps TargetCorps;
        }

    }



    // メインに置く
    public static partial class TargetSensorHolderLink
    {
        public struct HolderLinkData : IComponentData
        {
            public Entity HolderEntity;
        }
    }

    // 所属グループ
    static public partial class CorpsGroup
    {
        public struct Data : IComponentData
        {
            public Corps BelongTo;
        }

    }




    // 移動先関連 -----------------------------------------------------------


    //public static partial class TargetSensor
    //{
    //    public struct MoveTargetLinkData : IComponentData
    //    {
    //        public Entity MoveTargetEnity;
    //    }
    //}

    // ホルダー
    public static partial class TargetSensorHolder
    {
        [InternalBufferCapacity(3)]
        public struct SensorLinkData : IBufferElementData
        {
            public Entity SensorEntity;
            public float Interval;
        }
        [InternalBufferCapacity(3)]
        public struct SensorNextTimeData : IBufferElementData
        {
            public float NextTime;
        }
    }

    // センサー
    public static partial class TargetSensor
    {
        public struct LinkTargetMainData : IComponentData
        {
            public Entity TargetMainEntity;
        }

        public struct WakeupFindTag : IComponentData
        { }
        public struct AcqurireTag : IComponentData
        { }

        //public struct CurrentData : IComponentData
        //{
        //    public int LastFrame;
        //    public float3 Position;
        //}

        public struct GroupFilterData : IComponentData
        {
            public uint CollidesWith;
        }

        public struct CollisionData : IComponentData
        {
            public Entity PostureEntity;
            public CollisionFilter Filter;
            public float Distance;
            public Corps CorpsJoin;
        }
    }

    // センサー及びホルダー
    public static partial class TargetSensorResponse
    {
        public struct SensorMainTag : IComponentData
        { }

        public struct PositionData : IComponentData
        {
            public float3 Position;
        }
    }
}
