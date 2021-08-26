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
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;

namespace DotsLite.Draw
{



    static public partial class BoneDraw
    {

        //public struct WithLodTag : IComponentData
        //{ }

        public struct LinkData : IComponentData
        {
            public Entity DrawModelEntityCurrent;
            public Entity DrawInstanceEntity;
        }

        public struct IndexData : IComponentData
        {
            public int BoneId;
            public int BoneLength;
        }

        public unsafe struct TargetWorkData : IComponentData
        {
            public int DrawInstanceId;
        }

    }

    public static class Tf
    {
        /// <summary>
        /// ボーン変形に先立って、各軸方向にスケーリングさせる。
        /// </summary>
        public struct PreScalingData : IComponentData
        {
            public float4 Scale;
        }
    }
}

namespace DotsLite.Model
{

    // ボーンのベクトル数も兼ねている
    public enum BoneType
    {
        ST = 2,
        SRT = 3,
        T = 1,
        RT = 2,
        RTS = 3,
        Matrix4x3 = 3,
        PtoP = 2,
        P1uv = 2,
        PtoPuv = 3,
    }

    //static public partial class TransformOption
    //{

    //    /// <summary>
    //    /// トランスフォームをやらない。スリープなどで使用。
    //    /// job_per_depth では bone entity に、reelup_chain では posture entity につける。
    //    /// </summary>
    //    public struct ExcludeTransformTag : IComponentData
    //    { }

    //    /// <summary>
    //    /// トランスフォームを強制する。パーツごとの静的コライダなどで有用か？
    //    /// job_per_depth では bone entity に、reelup_chain では posture entity につける。
    //    /// </summary>
    //    public struct ForceTransformTag : IComponentData
    //    { }

    //}

    static public partial class Bone
    {

        public struct TransformTargetTag : IComponentData
        { }


        public struct PostureLinkData : IComponentData
        {
            public Entity PostureEntity;
        }


        public struct RelationLinkData : IComponentData
        {
            public Entity NextBoneEntity;
            public Entity ParentBoneEntity;
        }

        public struct MotionBlendLinkData : IComponentData
        {
            public Entity MotionBlendEntity;
        }

        public struct Stream0LinkData : IComponentData
        {
            public Entity PositionStreamEntity;
            public Entity RotationStreamEntity;
        }
        public struct Stream1LinkData : IComponentData
        {
            public Entity PositionStreamEntity;
            public Entity RotationStreamEntity;
        }
        public struct Stream2LinkData : IComponentData
        {
            public Entity PositionStreamEntity;
            public Entity RotationStreamEntity;
        }


        public struct LocalValueData : IComponentData
        {
            public float3 Position;
            public quaternion Rotation;
        }


        public struct InitializeData : IComponentData
        {
            public Entity PostureEntity;
        }



        public interface ILvLinkData
        {
            Entity GetParentBoneEntity { get; }
        }
        public struct Lv01LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv02LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv03LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv04LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv05LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv06LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv07LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv08LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
        public struct Lv09LinkData : IComponentData, Bone.ILvLinkData
        {
            public Entity ParentBoneEntity;
            public Entity GetParentBoneEntity { get => this.ParentBoneEntity; }
        }
    }
}