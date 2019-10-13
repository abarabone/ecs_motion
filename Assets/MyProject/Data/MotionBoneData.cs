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
using Abss.Obj.Entities;

namespace Abss.Motion
{

    public struct BoneTransformLinkData : IComponentData
    {
        public Entity NextEntity;
        public Entity ParentBoneEntity;
    }

    public struct BoneStreamLinkData : IComponentData
    {
        public Entity PositionStreamEntity;
        public Entity RotationStreamEntity;
    }
    
    public struct BoneDrawLinkData : IComponentData
    {
        public Entity DrawEntity;
    }

    public struct BoneDrawTargetIndexData : IComponentData
    {
        public int ModelIndex;
        //public int InstanceIndex;
        public int BoneId;
        //public int BoneLength;
        public int InstanceBoneOffset;
    }
}