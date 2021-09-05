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
using DotsLite.Geometry;
using System.Runtime.InteropServices;
using System;

namespace DotsLite.Draw
{
    using DotsLite.Utilities;


    //public struct DrawInstanceEntity :
    //    ITypedEntity<
    //        DrawInstance.ModelLinkData,
    //        DrawInstance.TargetWorkData
    //    >
    //{
    //    public Entity Entity { get; set; }

    //    static public implicit operator DrawInstanceEntity( Entity ent )
    //        => new DrawInstanceEntity { Entity = ent };
    //}



    static public partial class DrawInstance
    {

        /// <summary>
        /// 
        /// </summary>
        public struct MeshTag : IComponentData
        { }

        /// <summary>
        /// トランスフォームが必要な draw instance であることを示す
        /// </summary>
        public struct BoneModelTag : IComponentData
        { }


        /// <summary>
        /// 描画モデルの種類情報
        /// </summary>
        public struct ModelLinkData : IComponentData
        {
            public Entity DrawModelEntityCurrent;
        }
        public struct PostureLinkData : IComponentData
        {
            public Entity PostureEntity;
        }
        public struct BoneLinkData : IComponentData
        {
            public Entity BoneRelationTop;
        }


        // posture とルートボーンの間にかませる、ルート位置補正
        // draw instance entity の translation/rotation に変換を持たせる
        public struct TransformOffsetData : IComponentData
        {
            public float3 Position;
            public quaternion Rotation;
        }
        // いずれはスケーリングに対応させるべく、マトリクス版も必要になるだろう
        //public struct TransformOffsetMatrixData : IComponentData
        //{
        //    public float4x3 Matrix;
        //}



        public struct TargetWorkData : IComponentData
        {
            public int DrawInstanceId;   // -1 なら描画しない
        }

        public struct ModelLod2LinkData : IComponentData
        {
            public Entity DrawModelEntityNear;
            public Entity DrawModelEntityFar;
            public float LimitDistanceSqrNear;
            public float LimitDistanceSqrFar;
            public float MarginDistanceSqrNear;
            public float MarginDistanceSqrFar;
        }

        public struct ModelLod1LinkData : IComponentData
        {
            public Entity DrawModelEntityNear;
            public float LimitDistanceSqrNear;
            public float MarginDistanceSqrNear;
        }


        /// <summary>
        /// 
        /// </summary>
        public struct NeedLodCurrentTag : IComponentData
        { }
        public struct LodCurrentIsFarTag : IComponentData
        { }
        public struct LodCurrentIsNearTag : IComponentData
        { }
        public struct LodCurrentIsNothingTag : IComponentData
        { }

        ///// <summary>
        ///// カリング用オブジェクトＡＡＢＢ
        ///// </summary>
        //public struct TargetAabb : IComponentData
        //{
        //    public float4 min;
        //    public float4 max;
        //}

        ///// <summary>
        ///// カリング用オブジェクト球データ
        ///// </summary>
        //public struct TargetSphere : IComponentData
        //{
        //    public float center;
        //    public float radius;
        //}



    }

}
