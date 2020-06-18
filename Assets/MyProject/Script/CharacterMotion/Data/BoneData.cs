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

using Abarabone.Geometry;

namespace Abarabone.Motion
{

    public struct BoneRelationLinkData : IComponentData
    {
        public Entity NextBoneEntity;
        public Entity ParentBoneEntity;
    }

    public struct BoneMotionBlendLinkData : IComponentData
    {
        public Entity MotionBlendEntity;
    }

    public struct BoneStream0LinkData : IComponentData
    {
        public Entity PositionStreamEntity;
        public Entity RotationStreamEntity;
    }
    public struct BoneStream1LinkData : IComponentData
    {
        public Entity PositionStreamEntity;
        public Entity RotationStreamEntity;
    }
    public struct BoneStream2LinkData : IComponentData
    {
        public Entity PositionStreamEntity;
        public Entity RotationStreamEntity;
    }


    public struct BoneLocalValueData : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }


    public struct BoneInitializeData : IComponentData
    {
        public Entity PostureEntity;
    }


    
    public interface IBoneLvLinkData
    {
        Entity GetParentBoneEntity { get; }
    }
    public struct BoneLv01LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv02LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv03LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv04LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv05LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv06LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv07LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv08LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
    public struct BoneLv09LinkData : IComponentData, IBoneLvLinkData
    {
        public Entity ParentBoneEntity;
        public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
    }
}