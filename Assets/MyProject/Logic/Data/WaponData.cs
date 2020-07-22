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

        public struct FireUnitData : IComponentData
        {
            public Entity MainEntity;
            public Entity BulletEntity;
        }

        public struct BeamUnitData : IComponentData
        {
            public Entity PsylliumPrefab;

            public Entity MainEntity;
            public Entity BeamInstanceEntity;

            public float3 MuzzlePositionLocal;
        }
    }

    
    static public partial class Bullet
    {

        public struct BeamTag : IComponentData
        { }

        public struct BulletData : IComponentData
        {
            //public float3 Forward;

            public float LifeTime;
            public float InvTotalTime;
        }

    }


}
