using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

using DotsLite.Utilities;

namespace DotsLite.Character
{



    static public partial class Control
    {
        public struct MoveData : IComponentData
        {
            public float3 MoveDirection;
            public float VerticalAngle;

            public quaternion LookRotation;
            public quaternion BodyRotation;
            public float3 BodyUp;

            public float3 LookDirection => math.forward(this.LookRotation);
            public float3 BodyDirection => math.forward(this.BodyRotation);
        }

        public struct ActionData : IComponentData
        {
            public float JumpForce;
            public int actionbits;

            public bool IsChangeMotion { get => this.actionbits.get(0); set => this.actionbits.set(0, value); }
            public bool IsShooting { get => this.actionbits.get(1); set => this.actionbits.set(1, value); }
            public bool IsTriggerdSub { get => this.actionbits.get(2); set => this.actionbits.set(2, value); }
            public bool IsChangingWapon { get => this.actionbits.get(3); set => this.actionbits.set(3, value); }
        }

        public struct WorkData : IComponentData
        {
            public quaternion hrot;
            public float vangle;
        }

        public struct ActionLinkData : IComponentData
        {
            public Entity ActionEntity;
        }
    }

    static class bits
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool get(ref this int f, int i) => Convert.ToBoolean(f & 1 << i);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void set(ref this int f, int i, bool value) => f |= Convert.ToInt32(value) << i;
    }


    // 
    //public struct MoveHandlingData : IComponentData
    //{
    //    public ControlActionUnit ControlAction;
    //}

    //public struct ControlActionUnit
    //{
    //    public float3 MoveDirection;

    //    public quaternion LookRotation;
    //    public quaternion BodyRotation;
    //    public float VerticalAngle;

    //    public float3 BodyUp;
    //    public float3 LookDirection => math.forward(this.LookRotation);
    //    public float3 BodyDirection => math.forward(this.BodyRotation);

    //    public float JumpForce;
    //    public bool IsChangeMotion;
    //    public bool IsShooting;
    //    public bool IsTriggerdSub;
    //    public bool IsChangingWapon;

    //}


    static public partial class Move
    {
        public struct TurnParamaterData : IComponentData
        {
            public float TurnRadPerSec;
        }

        public struct SpeedParamaterData : IComponentData
        {
            public float SpeedPerSecMax;
            public float SpeedPerSec;
        }

        public struct EasingSpeedData : IComponentData
        {
            public float TargetSpeedPerSec;
            public float Rate;
        }
    }



    // アクション ---------------------------------------------

    public static partial class CharacterAction
    {

        //public struct LinkData : IComponentData
        //{
        //    public Entity MainEntity;
        //    public Entity MotionEntity;
        //}
            



    }


    // キャラクタアクションのステート -------------------------------------

    public struct MinicWalkActionState : IComponentData
    {
        public int Phase;
    }

    public struct SoldierWalkActionState : IComponentData
    {
        public int Phase;
    }




    // 多種コンポーネント兼用 -------------------------------------

    public struct PlayerTag : IComponentData
    { }

    public struct AntTag : IComponentData
    { }


    // 当たり判定 -----------------------------------------------

    static public partial class Grouding
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


}
