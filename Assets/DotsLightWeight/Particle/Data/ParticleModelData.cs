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

namespace DotsLite.ParticleSystem
{
    using DotsLite.Utilities;
    using DotsLite.Draw;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;



    public static partial class BillboadModel
    {
        public struct UvInformationData : IComponentData
        {
            public uint2 Division;
        }

        public struct IndexToUvData : IComponentData
        {
            public float2 CellSpan;
        }
    }


}
