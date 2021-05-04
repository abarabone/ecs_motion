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
    using Targeting;

    public enum HitType
    {
        none,
        charactor,
        part,

    }


    static public partial class Hit
    {

        // コリジョンを持つエンティティに付け、ヒット処理の適用対象に誘導する。
        public struct TargetData : IComponentData
        {
            public Entity MainEntity;
            public HitType HitType;
            public Corps Corps;// とりあえずここに一緒に
        }

    }
}


namespace DotsLite.Character
{

    static public partial class Armor
    {


        public struct SimpleDamageData : IComponentData
        {
            public float Durability;
        }

    }

    static public partial class CharacterAction
    {

        public struct DamageState : IComponentData
        {
            public int Phase;
            public float EndTime;
            public float Damage;
            public float4 DamageForce;
        }

        public struct DeadState : IComponentData
        {
            public int Phase;
            public float RemoveTime;
        }

    }

}