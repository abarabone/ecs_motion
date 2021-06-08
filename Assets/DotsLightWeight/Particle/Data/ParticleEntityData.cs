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
using System.Runtime.InteropServices;

namespace DotsLite.Draw
{

    static public partial class DrawInstance
    {
        public struct BillBoadTag : IComponentData
        { }

        public struct PsylliumTag : IComponentData
        { }
    }

}

namespace DotsLite.Particle
{
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;


    //public struct PsylliumEntity :
    //    ITypedEntity<
    //        DrawInstance.ParticleTag,
    //        DrawInstance.ModelLinkData,//DrawTransform.LinkData
    //        DrawInstance.TargetWorkData,
    //        Translation,
    //        Rotation
    //    >
    //{
    //    public Entity Entity { get; set; }

    //    static public implicit operator PsylliumEntity( Entity ent )
    //        => new PsylliumEntity { Entity = ent };
    //}

    //public struct LineParticleNodeEntity :
    //    ITypedEntity<
    //        Particle.LineParticlePointNodeLinkData,
    //        BoneDraw.LinkData,
    //        BoneDraw.IndexData,
    //        BoneDraw.TargetWorkData,
    //        Translation
    //    >
    //{
    //    public Entity Entity { get; set; }

    //    static public implicit operator LineParticleNodeEntity ( Entity ent )
    //        => new LineParticleNodeEntity { Entity = ent };
    //}



    static public partial class Particle
    {

        public struct LineParticlePointNodeLinkData : IComponentData
        {
            //public LineParticleNodeEntity NextNodeEntity;
            public Entity NextNodeEntity;
        }


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

        [InternalBufferCapacity(8)]
        public struct TranslationTailsData : IBufferElementData
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

        public struct AdditionalData : IComponentData
        {
            public Color32 Color;
            public float Size;
        }

        static public unsafe uint ToUint(this Color32 c) => new Color32Convertor { Color = c }.Uint;// (uint)(c.r << 24 | c.g << 16 | c.b << 8 | c.a << 0);

        static public Color32 ToColor32(this uint u) => new Color32Convertor { Uint = u }.Color;

        [StructLayout(LayoutKind.Explicit)]
        public struct Color32Convertor
        {
            [FieldOffset(0)] public Color32 Color;
            [FieldOffset(0)] public uint Uint;
        }
    }


}
