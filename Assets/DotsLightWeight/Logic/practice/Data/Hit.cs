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

namespace DotsLite.Collision
{

    static public partial class Hit
    {

        public enum HitType
        {
            none,
            charactor,
            part,

        }


        // コリジョンを持つエンティティに付け、ヒット処理の適用対象に誘導する。
        public struct TargetData : IComponentData
        {
            public Entity MainEntity;
            public HitType HitType;
        }


    }

}