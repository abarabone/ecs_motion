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

namespace Abarabone.Draw
{

    static public partial class DrawInstance
    {
        public struct ParticleTag : IComponentData
        { }
    }

}

namespace Abarabone.Particle
{
    using Abarabone.Utilities;
    using Abarabone.Draw;
    using Abarabone.Character;
    using Abarabone.CharacterMotion;


    public struct PsylliumEntity :
        ITypedEntity<
            DrawInstance.ParticleTag,
            DrawInstance.ModeLinkData,//DrawTransform.LinkData
            DrawInstance.TargetWorkData,
            Translation,
            Rotation
        >
    {
        public Entity Entity { get; set; }

        static public implicit operator PsylliumEntity( Entity ent )
            => new PsylliumEntity { Entity = ent };
    }

    public struct LineParticleNodeEntity :
        ITypedEntity<
            Particle.LineParticlePointNodeLinkData,
            DrawTransform.LinkData,
            DrawTransform.IndexData,
            DrawTransform.TargetWorkData,
            Translation
        >
    {
        public Entity Entity { get; set; }

        static public implicit operator LineParticleNodeEntity ( Entity ent )
            => new LineParticleNodeEntity { Entity = ent };
    }



    static public partial class Particle
    {

        public struct LineParticlePointNodeLinkData : IComponentData
        {
            public LineParticleNodeEntity NextNodeEntity;
        }


        public struct TranslationPtoPData : IComponentData
        {
            public float3 Start;
            public float3 End;
        }

        public struct AdditionalData : IComponentData
        {
            public Color32 Color;
            public float Size;
        }

        static public uint ToUint(this Color32 c) => (uint)(c.r << 24 | c.g << 16 | c.b << 8 | c.a << 0);

    }


}
