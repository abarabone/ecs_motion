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
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;
using System.Runtime.InteropServices;

namespace DotsLite.Draw
{

    static public partial class DrawInstance
    {
        public struct BillBoadTag : IComponentData
        { }

        public struct PsylliumTag : IComponentData
        { }

        public struct LineParticleTag : IComponentData
        { }
    }

}

namespace DotsLite.ParticleSystem
{
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;



    public static partial class Particle
    {

        public struct LineParticlePointNodeLinkData : IComponentData
        {
            //public LineParticleNodeEntity NextNodeEntity;
            public Entity NextNodeEntity;
        }

        public struct OptionalData : IComponentData
        {
            public Color32 BlendColor;
            public Color32 AdditiveColor;
            public float Radius;
        }
        // blend : src.rgb * src.a + dst.rgb * (1 - src.a)
        // add   : src.rgb * src.a * dst.rgb
        // src は tex や col、dst は レンダーテクスチャ等の画面

        public struct VelocityFactorData : IComponentData
        {
            public float4 PrePosition;
        }
        public struct VelocitySpecData : IComponentData
        {
            public float4 AccelerationAndGravityFactor;
            public float3 Acceleration
            {
                get => this.AccelerationAndGravityFactor.xyz;
                set => this.AccelerationAndGravityFactor = value.As_float4(this.GravityFactor);
            }
            public float GravityFactor
            {
                get => this.AccelerationAndGravityFactor.w;
                set => this.AccelerationAndGravityFactor.w = value;
            }
        }


        public struct LifeTimeInitializeTag : IComponentData
        { }

        // 破棄時刻指定
        public struct LifeTimeData : IComponentData
        {
            public float StartTime;
            //public float EndTime;
        }
        public struct LifeTimeSpecData : IComponentData
        {
            public float DurationSec;
        }


        public struct EasingSetting : IComponentData
        {
            public float LastDistanceMin;
            public float LastDistanceMax;
            //public float Rate;
        }
        public struct EasingData : IComponentData
        {
            public float4 LastPositionAndRate;

            public float3 LastPosition
            {
                set => this.LastPositionAndRate = value.As_float4(this.Rate);
                get => this.LastPositionAndRate.xyz;
            }
            public float Rate
            {
                set => this.LastPositionAndRate.w = value;
                get => this.LastPositionAndRate.w;
            }
            public void Set(float3 pos, float rate) => this.LastPositionAndRate = pos.As_float4(rate);
        }

    }



}
