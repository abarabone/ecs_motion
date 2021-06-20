using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


namespace DotsLite.Particle
{
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;



    static public partial class LineParticle
    {


        [InternalBufferCapacity(8)]
        public struct TranslationTailLineData : IBufferElementData
        {
            public float4 PositionAndColor;

            public float3 Position
            {
                get => this.PositionAndColor.xyz;
                //set => this.PositionAndColor.xyz = value;
                set => this.PositionAndColor = new float4(value, math.asfloat(this.Color.ToUint()));
            }
            public Color32 Color
            {
                get => math.asuint(this.PositionAndColor.w).ToColor32();
                //set => this.PositionAndColor.w = value;
                set => this.PositionAndColor = new float4(this.Position, math.asfloat(value.ToUint()));
            }
        }

    }

    static public class Spring
    {

        public struct SpecData : IComponentData
        {
            public float Spring;
            public float Dumper;
            public float Rest;
            public float GravityFactor;
        }

        [InternalBufferCapacity(8)]
        public struct StatesData : IBufferElementData
        {
            public float4 PrePosition;
        }

        public struct StickyApplyData : IComponentData
        {
            public float FirstFactor;
            public float LastFactor;
        }

        public struct HittableSegmentData : IComponentData
        {
            public int2 Index;
        }

        public struct StickyStateData : IComponentData
        {
            public NextStickyMode NextSticky;
            public int PointLength;
        }
        public enum NextStickyMode
        {
            none = 0,
            first = 1,
            last = 2,
            completed = 3,
        }

        // ボーンエンティティで実装する場合
        //public struct StickyTREntityData : IComponentData
        //{
        //    public Entity Target;
        //    public float4 LocalPosition;
        //}
        //public struct StickyTEntityData : IComponentData
        //{
        //    public Entity Target;
        //}
        //public struct StickyPointData : IComponentData
        //{
        //    public float4 Position;
        //}


        public struct StickySelfFirstTag : IComponentData
        { }

        public struct StickyTREntityFirstData : IComponentData
        {
            public Entity Target;
            public float4 LocalPosition;
        }
        public struct StickyTREntityLastData : IComponentData
        {
            public Entity Target;
            public float4 LocalPosition;
        }

        public struct StickyTEntityFirstData : IComponentData
        {
            public Entity Target;
        }
        public struct StickyTEntityLastData : IComponentData
        {
            public Entity Target;
        }

        public struct StickyPointFirstData : IComponentData
        {
            public float4 Position;
        }
        public struct StickyPointLastData : IComponentData
        {
            public float4 Position;
        }
    }
}

