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
using Abarabone.Geometry;

namespace Abarabone.Character
{
	
    public struct PostureLinkData : IComponentData
    {
        public Entity BoneRelationTop;
    }

    public struct PostureNeedTransformTag : IComponentData
    { }

	public struct PostureUniqueIdData : IComponentData
    {
        public int UniqueId;
    }



    public struct ControlActionUnit
    {
        public float3 MoveDirection;

        public quaternion LookRotation;
        public quaternion HorizontalRotation;
        public float VerticalAngle;

        public float3 LookDirection => math.forward( this.LookRotation );

        public float JumpForce;
        public bool IsChangeMotion;

        public float3 Up;
    }

    public struct MoveHandlingData : IComponentData
    {
        public ControlActionUnit ControlAction;
    }

}