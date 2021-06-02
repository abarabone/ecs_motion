using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace DotsLite.Arms.Authoring
{
    public class MuzzleLocalPositionAuthoring : MonoBehaviour, IMuzzleLocalPostion
    {
        public float3 MuzzleLocalPosition;
        public float3 Local => this.MuzzleLocalPosition;
        public bool UseEffect => false;
    }
}
