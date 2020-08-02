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

        public struct BulletEmitterData : IComponentData
        {
            public Entity BulletEntity;

            public Entity MainEntity;
        }

        public struct BeamEmitterData : IComponentData
        {
            public Entity BeamPrefab;

            public Entity MainEntity;
            public Entity BeamInstanceEntity;

            public float3 MuzzlePositionLocal;
        }
    }

}
