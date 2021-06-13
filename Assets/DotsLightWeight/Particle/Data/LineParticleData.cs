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


        [InternalBufferCapacity(6)]
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
        }

        [InternalBufferCapacity(8)]
        public struct StateData : IComponentData
        {
            public float4 PrePosition;
        }

    }
}

