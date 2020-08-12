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

    static public partial class FunctionUnitWithWapon
    {

        public struct WaponCarryIdData : IComponentData
        {
            public int WaponCarryId;
        }
        public struct SelectorLinkData : IComponentData
        {
            public Entity SelectorEntity;
        }
        //public struct UnitChainLinkData : IComponentData
        //{
        //    public Entity NextUnitEntity;
        //}

        public struct TriggerTypeData : IComponentData
        {
            public TriggerType Type;
        }
        public enum TriggerType
        {
            main,
            sub
        }

        public struct InitializeData : IComponentData
        {
            public int WaponCarryId;
            public Entity SelectorEntity;
            public Entity OwnerMainEntity;
            public Entity MuzzleBodyEntity;
        }
    }

    static public partial class FunctionUnitWithDirect
    {

    }

    /// <summary>
    /// 兵士はユニットを武器にまとめて所持する。
    /// 敵はユニット単位で武器を使用する。
    /// ユニットは「機能をトリガーすること」と「反射間隔／リロードの制御」以外は行わない。
    /// 他はユニットを管理する側の役目。
    /// </summary>
    static public partial class FunctionUnit
    {

        public struct OwnerLinkData : IComponentData
        {
            public Entity OwnerMainEntity;       // 当たり判定で自身を除外する、トリガー判定、など
            public Entity MuzzleBodyEntity; // 射出の向きやエフェクトの位置のため
        }
        public struct BulletEmittingData : IComponentData
        {
            public Entity BulletPrefab;

            public float3 MuzzlePositionLocal;

            public float EmittingInterval;
            public float AccuracyRad;
            public int NumEmitMultiple;
            public float RangeDistanceFactor;// 切り替え時に弾丸のと計算確定しようかと思ったが、わかりにくいのでやめた
        }

        public struct TriggerData : IComponentData
        {
            public bool IsTriggered;
        }

        public struct SightModeData : IComponentData
        {
            public bool IsCameraSight;
        }
        public struct EmittingStateData : IComponentData
        {
            public double NextEmitableTime;
        }

        public struct RealoadingStateData : IComponentData
        {

        }


        //public struct BulletEmitterData : IComponentData
        //{
        //    public Entity BulletEntity;

        //    public Entity MainEntity;
        //}

        //public struct BeamEmitterData : IComponentData
        //{
        //    public Entity BeamPrefab;

        //    public Entity MainEntity;

        //    public float3 MuzzlePositionLocal;
        //}
    }

}
