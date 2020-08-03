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

    static public partial class Wapon
    {

        public struct BulletEmittingData : IComponentData
        {
            public Entity BulletPrefab;
            public Entity MainEntity;

            public Entity MuzzleBodyEntity;
            public float3 MuzzlePositionLocal;

            public float EmittingInterval;
            public float DirectionAccuracy;
            public int NumEmitMultiple;
            public float RangeDistanceFactor;// 切り替え時に弾丸のと計算確定しようかと思ったが、わかりにくいのでやめた
        }

        public struct SightModeData : IComponentData
        {
            public bool IsCameraSight;
        }
        public struct EmittingStateData : IComponentData
        {
            public float RestEmittingInterval;
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
