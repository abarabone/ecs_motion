﻿using System.Collections;
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

namespace Abss.Motion
{

    public struct BoneRelationLinkData : IComponentData
    {
        public Entity NextBoneEntity;
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

    public struct BoneIdData : IComponentData
    {
        public int BoneId;
    }

    public struct BoneDrawTargetIndexWorkData : IComponentData
    {
        public int ModelIndex;
        public int InstanceBoneOffset;
    }


    public struct BoneLv00Data : IComponentData
    {
        
    }
    public struct BoneLv01Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv02Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv03Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv04Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv05Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv06Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv07Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv08Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLv09Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
}