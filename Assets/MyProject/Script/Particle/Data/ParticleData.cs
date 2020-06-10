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
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;

using Abarabone.Utilities;
using Abarabone.Draw;
using Abarabone.Character;
using Abarabone.Motion;

namespace Abarabone.Particle
{

    public struct PsylliumEntity :
        ITypedEntity<
            ParticleTag,
            DrawInstanceModeLinkData,//DrawTransformLinkData
            DrawInstanceTargetWorkData,
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
            LineParticlePointNodeLinkData,
            DrawTransformLinkData,
            DrawTransformIndexData,
            DrawTransformTargetWorkData,
            Translation
        >
    {
        public Entity Entity { get; set; }

        static public implicit operator LineParticleNodeEntity ( Entity ent )
            => new LineParticleNodeEntity { Entity = ent };
    }





    public struct ParticleTag : IComponentData
    { }


    public struct LineParticlePointNodeLinkData : IComponentData
    {
        public LineParticleNodeEntity NextNodeEntity;
    }




}
