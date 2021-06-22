using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

namespace DotsLite.Arms.Authoring
{
    using DotsLite.Utilities;

    public class MuzzleLocalPositionAuthoring : MonoBehaviour, IMuzzleLocalPostion
    {
        public float3 MuzzleLocalPosition;
        public float3 Local => this.MuzzleLocalPosition;
        public bool UseEffect => false;
    }
}
