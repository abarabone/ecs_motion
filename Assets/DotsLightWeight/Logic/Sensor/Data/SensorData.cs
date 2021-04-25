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

namespace Abarabone.Targeting
{

    // ���C���ɒu��
    public static partial class TargetSensorHolderLink
    {
        public struct HolderLinkData : IComponentData
        {
            public Entity HolderEntity;
        }
    }

    // �����O���[�v
    public static partial class TargetGroup
    {
        public struct BelongToData : IComponentData
        {
            public int BelongTo;
        }
    }



    // �_���[�W�Ȃ� -----------------------------------------------------

    public static class Life
    {
        public struct SimpleDamageData : IComponentData
        {
            public float Durability;
        }
    }


    // �ړ���֘A -----------------------------------------------------------


    //public static partial class TargetSensor
    //{
    //    public struct MoveTargetLinkData : IComponentData
    //    {
    //        public Entity MoveTargetEnity;
    //    }
    //}

    // �z���_�[
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

    // �Z���T�[
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
            public float Distance;
            public CollisionFilter Filter;
        }
    }

    // �Z���T�[�y�уz���_�[
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