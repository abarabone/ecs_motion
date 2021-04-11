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

using Abarabone.Utilities;

namespace Abarabone.Character
{






    // 

    public struct MoveHandlingData : IComponentData
    {
        public ControlActionUnit ControlAction;
    }

    public struct ControlActionUnit
    {
        public float3 MoveDirection;

        public quaternion LookRotation;
        public quaternion HorizontalRotation;
        public float VerticalAngle;

        public float3 LookDirection => math.forward(this.LookRotation);

        public float JumpForce;
        public bool IsChangeMotion;
        public bool IsShooting;
        public bool IsTriggerdSub;
        public bool IsChangingWapon;

        public float3 Up;
    }



    // 所属グループ
    public static partial class TargetGroup
    {
        public struct BelongToData : IComponentData
        {
            public int BelongTo;
        }
    }



    // 多種コンポーネント兼用 -------------------------------------

    public struct PlayerTag : IComponentData
    { }

    public struct AntTag : IComponentData
    { }


    // 当たり判定 -----------------------------------------------

    static public class GroudHit
    {

    }

    public struct GroundHitResultData : IComponentData
    {
        public bool IsGround;
    }
    public struct GroundHitSphereData : IComponentData
    {
        public float3 Center;
        public float Distance;
        public CollisionFilter Filter;
    }
    public struct GroundHitRayData : IComponentData
    {
        public float3 Start;
        public DirectionAndLength Ray;
        public CollisionFilter Filter;
    }
    //public struct GroundHitColliderData : IComponentData
    //{
    //    public BlobAssetReference<Unity.Physics.Collider> Collider;
    //}

    public struct GroundHitWallingData : IComponentData
    {
        public float CenterHeight;
        public float HangerRange;
        public CollisionFilter Filter;
    }

    // 壁移動させたいメインエンティティに付けておく
    public struct WallingTag : IComponentData
    { }
    public struct WallHangingData : IComponentData
    {
        public WallingState State;
        public enum WallingState
        {
            none_rotating,
            front_45_rotating,
        }
    }
    // 接触していない時に付き、接触判定に使用される（つけ外ししないほうがチャンク移動減ってよいか？？）
    public struct WallHitResultData : IComponentData
    {
        public bool IsHit;
    }


    // 水平移動させたいメインエンティティに付けておく
    public struct HorizontalMovingTag : IComponentData
    { }


    // キャラクタアクションのステート -------------------------------------

    public struct MinicWalkActionState : IComponentData
    {
        public int Phase;
    }

    public struct SoldierWalkActionState : IComponentData
    {
        public int Phase;
    }

    public struct AntWalkActionState : IComponentData
    {
        public int Phase;
    }
}


namespace Abarabone.Targeting
{

    // ダメージなど -----------------------------------------------------

    public static class Life
    {
        public struct SimpleDamageData : IComponentData
        {
            public float Durability;
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
        public struct SensorsLinkData : IBufferElementData
        {
            public Entity SensorEntity;
            public float LastTime;
            public float Interval;
        }
    }

    // センサー
    public static partial class TargetSensor
    {
        public struct LinkMainData : IComponentData
        {
            public Entity MainEntity;
        }

        public struct PollingTag : IComponentData
        { }

        //public struct CurrentData : IComponentData
        //{
        //    public int LastFrame;
        //    public float3 Position;
        //}

        public struct GroupFilterData : IComponentData
        {
            public int CollidesWith;
        }

        public struct CollisionData : IComponentData
        {
            public float Distance;
            public CollisionFilter Filter;
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
