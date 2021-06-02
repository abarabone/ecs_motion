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
    /// <summary>
    /// 兵士はユニットを武器にまとめて所持する。
    /// 敵はユニット単位で武器を使用する。
    /// ユニットは「機能を発動する（射出など）」と「反射間隔／有効無効」以外は行わない。
    /// 他はユニットを管理する側の役目。
    /// </summary>
    static public partial class FunctionUnit
    {

        ////public struct OwnerLinkData : IComponentData
        ////{
        ////    //public Entity OwnerMainEntity;  // 当たり判定で自身を除外するなど
        ////    public Entity MuzzleEntity;     // 射出の向きやエフェクトの位置のため
        ////}
        //public struct StateLinkData : IComponentData
        //{
        //    public Entity StateEntity;
        //}
        //public struct MuzzleLinkData : IComponentData
        //{
        //    public Entity EmitterEntity;    // 射出の向き
        //    public Entity MuzzleEntity;     // エフェクトの位置
        //}
        public struct holderLinkData : IComponentData
        {
            public Entity WaponHolderEntity;
        }
        //public struct BulletEmittingData : IComponentData
        //{
        //    public Entity BulletPrefab;
        //    public Entity EffectPrefab;

        //    public float3 MuzzlePositionLocal;

        //    public float EmittingInterval;
        //    public float AccuracyRad;
        //    public int NumEmitMultiple;
        //    public float RangeDistanceFactor;// 切り替え時に弾丸のと計算確定しようかと思ったが、わかりにくいのでやめた
        //}

        //public struct TriggerData : IComponentData
        //{
        //    public bool IsTriggered;
        //}

        ////public struct SightModeData : IComponentData
        ////{
        ////    public bool IsCameraSight;
        ////}
        //public struct EmittingStateData : IComponentData
        //{
        //    public float NextEmitableTime;
        //}

        public struct PrevFrameMuzzlePosition : IComponentData
        {
            public float4 PrevPosition;
        }

        //public struct ActivateData : IComponentData
        //{
        //    public bool IsActive;   
        //}

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

    static public partial class FunctionUnitAiming
    {

        public struct ParentBoneLinkData : IComponentData
        {
            public Entity ParentEntity;
        }

        public struct HighAngleShotData : IComponentData
        {
            public Entity TargetPostureEntity;
            public float EndTime;
        }
    }






    // trial
    static public partial class Emitter
    {

        public struct OwnerLinkData : IComponentData
        {
            public Entity StateEntity;
        }

        //public struct MuzzleLinkData : IComponentData
        //{
        //    public Entity MuzzleEntity;
        //}


        //public struct EffectStateData : IComponentData
        //{
        //    public double NextEmitableTime;
        //}

        public struct StateData : IComponentData
        {
            public float NextEmitableTime;
            public int EmitFrequencyInCurrentFrame;
        }

        public struct EffectMuzzleLinkData : IComponentData
        {
            public Entity MuzzleEntity;
        }
        public struct EffectMuzzlePositionData : IComponentData
        {
            public float4 MuzzlePositionLocal;
        }
        public struct EffectEmittingData : IComponentData
        {
            public Entity Prefab;
        }

        public struct BulletMuzzleLinkData : IComponentData
        {
            public Entity MuzzleEntity;
        }
        public struct BulletMuzzlePositionData : IComponentData
        {
            public float4 MuzzlePositionLocal;
        }
        public struct BulletEmittingData : IComponentData
        {
            public Entity Prefab;

            public float EmittingInterval;
            public float AccuracyRad;
            public int NumEmitMultiple;
            public float RangeDistanceFactor;// 切り替え時に弾丸のと計算確定しようかと思ったが、わかりにくいのでやめた
        }

        public struct BulletEmittingWorkData : IComponentData
        {
            public float BulletLifeTime;    // 距離とスピードから算出した時間
            public float BulletRadius;      // bullet からの写し
        }

        public struct PrevFrameMuzzlePosition : IComponentData
        {
            public float4 PrevPosition;
        }

        public struct TriggerData : IComponentData
        {
            public bool IsTriggered;
        }

    }

}
