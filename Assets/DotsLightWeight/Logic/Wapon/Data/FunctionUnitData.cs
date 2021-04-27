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
    /// <summary>
    /// 兵士はユニットを武器にまとめて所持する。
    /// 敵はユニット単位で武器を使用する。
    /// ユニットは「機能を発動する（射出など）」と「反射間隔／有効無効」以外は行わない。
    /// 他はユニットを管理する側の役目。
    /// </summary>
    static public partial class FunctionUnit
    {

        //public struct OwnerLinkData : IComponentData
        //{
        //    //public Entity OwnerMainEntity;  // 当たり判定で自身を除外するなど
        //    public Entity MuzzleEntity;     // 射出の向きやエフェクトの位置のため
        //}
        public struct StateLinkData : IComponentData
        {
            public Entity StateEntity;
        }
        public struct MuzzleLinkData : IComponentData
        {
            public Entity EmitterEntity;    // 射出の向き
            public Entity MuzzleEntity;     // エフェクトの位置
        }
        public struct holderLinkData : IComponentData
        {
            public Entity WaponHolderEntity;
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

        //public struct SightModeData : IComponentData
        //{
        //    public bool IsCameraSight;
        //}
        public struct EmittingStateData : IComponentData
        {
            public double NextEmitableTime;
        }

        public struct ActivateData : IComponentData
        {
            public bool IsActive;   
        }

        public struct RealoadingStateData : IComponentData
        {
            
        }

    }

    static public partial class FunctionUnitInWapon
    {
        public struct TriggerSpecificData : IComponentData
        {
            public TriggerType Type;
            public int WaponCarryId;
        }
        public enum TriggerType
        {
            main,
            sub
        }
    }

}
