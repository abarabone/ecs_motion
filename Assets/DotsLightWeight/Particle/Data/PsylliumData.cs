using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


namespace DotsLite.ParticleSystem
{
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;



    static public partial class Psyllium
    {

        public struct MoveTailTag : IComponentData
        { }


        //public struct TranslationPtoPData : IComponentData
        //{
        //    public float3 Start;
        //    public float3 End;
        //}
        public struct TranslationTailData : IComponentData
        {
            public float4 PositionAndSize;

            public float3 Position
            {
                get => this.PositionAndSize.xyz;
                //set => this.PositionAndSize.xyz = value;
                set => this.PositionAndSize = new float4(value, this.Size);
            }
            public float Size
            {
                get => this.PositionAndSize.w;
                //set => this.PositionAndSize.w = value;
                set => this.PositionAndSize = new float4(this.Position, value);
            }
        }
    }
}