using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine.InputSystem;
using Unity.Physics.Systems;

namespace DotsLite.Character
{

    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;


    [GenerateAuthoringComponent]
    //public class CharacterFollowCameraPositionData : IComponentData
    public struct CharacterFollowCameraPositionData : IComponentData
    {

        public float3 RotationCenter;

        public float3 lookUpPosition;
        public float3 lookForwardPosition;
        public float3 lookDownPosition;

    }

}