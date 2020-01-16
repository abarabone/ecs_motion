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

using Abss.Utilities;
using Abss.Draw;
using Abss.Character;
using Abss.Motion;

namespace Abss.Particle
{



    public struct ParticleTag : IComponentData
    { }




    public struct LineParticlePointNodeLinkData : IComponentData
    {
        public ParticleNodeEntity NextNodeEntity;
    }




    public struct ParticleNodeEntity :
        ITypedEntity<
            LineParticlePointNodeLinkData,
            DrawTransformLinkData,
            DrawTransformIndexData,
            DrawTransformTargetWorkData,
            Translation
        >
    {
        public Entity Entity { get; set; }
    }

}
