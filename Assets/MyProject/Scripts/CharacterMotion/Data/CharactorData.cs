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
using Unity.Physics;

using Abss.Geometry;

namespace Abss.Character
{

    public struct CharacterLinkData : IComponentData
    {
        public Entity PostureEntity;
        public Entity DrawEntity;
        public Entity MainMotionEntity;// メインとかサブとかあるのはいまいちな気がするが、どうしようか
    }
    public struct CharacterSubMotion1LinkData : IComponentData
    {
        public Entity MotionEntity;
    }
    public struct CharacterSubMotion2LinkData : IComponentData
    {
        public Entity MotionEntity;
    }


    public struct GroundHitResultData : IComponentData
    {
        public bool IsGround;
    }
    public struct GroundHitSphereData : IComponentData
    {
        public float3 Center;
        public float Distance;
        public CollisionFilter filter;
    }
    public struct GroundHitRayData : IComponentData
    {
        public float3 Center;
        public float Distance;
        public CollisionFilter filter;
    }
    //public struct GroundHitColliderData : IComponentData
    //{
    //    public BlobAssetReference<Unity.Physics.Collider> Collider;
    //}


    // キャラクタアクションのステート -------------------------------------

    public struct MinicWalkActionState : IComponentData
    {
        public int Phase;
    }

    public struct SoldierWalkActionState : IComponentData
    {
        public int Phase;
    }



    // 多種コンポーネント兼用 -------------------------------------

    public struct PlayerTag : IComponentData
    { }

}

