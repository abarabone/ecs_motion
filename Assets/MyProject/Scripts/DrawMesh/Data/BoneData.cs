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

namespace Abss.Motion
{
    
    public struct BoneMotionLinkData : IComponentData
    {
        public Entity MotionEntity;
    }

    public struct BoneDrawLinkData : IComponentData
    {
        public Entity DrawEntity;
    }

    public struct BoneRelationLinkData : IComponentData
    {
        public Entity NextBoneEntity;
        public Entity ParentBoneEntity;
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

    public struct BoneIndexData : IComponentData
    {
        public int ModelIndex;
        public int BoneId;
    }

    public struct BoneDrawTargetIndexWorkData : IComponentData
    {
        public int InstanceBoneOffset;
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