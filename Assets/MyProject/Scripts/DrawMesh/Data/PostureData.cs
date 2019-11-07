using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using UniRx.Triggers;
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
using Abss.Geometry;

namespace Abss.Instance
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


    public struct MoveHandlingData : IComponentData
    {
        public ControlActionUnit ControlAction;
    }

}