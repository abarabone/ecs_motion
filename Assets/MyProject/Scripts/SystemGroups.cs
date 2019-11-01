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
using System.Runtime.InteropServices;

namespace Abss.SystemGroup
{

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class ObjectLogicSystemGroup : ComponentSystemGroup
    { }


    //[DisableAutoCreation]
    [UpdateAfter(typeof(ObjectLogicSystemGroup))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DrawPrevSystemGroup : ComponentSystemGroup
    { }

    //[DisableAutoCreation]
    [UpdateAfter(typeof(DrawPrevSystemGroup))]
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public class MotionSystemGroup : ComponentSystemGroup
    { }

    [UpdateAfter( typeof( DrawPrevSystemGroup ) )]
    [UpdateBefore( typeof( DrawSystemGroup ) )]
    [UpdateInGroup( typeof( PresentationSystemGroup ) )]
    public class DrawAllocationGroup : ComponentSystemGroup
    { }

    //[DisableAutoCreation]
    [UpdateAfter(typeof(MotionSystemGroup))]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DrawSystemGroup : ComponentSystemGroup
    { }


    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class BonePhysicsSystemGroup : ComponentSystemGroup
    { }
}