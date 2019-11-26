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
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;

using Abss.Geometry;

namespace Abss.Character
{


    // 多種コンポーネント兼用 -------------------------------------

    public struct PlayerTag : IComponentData
    { }

    public struct AntTag : IComponentData
    { }



    // キャラクタアクションのステート -------------------------------------

    public struct MinicWalkActionState : IComponentData
    {
        public int Phase;
    }

    public struct SoldierWalkActionState : IComponentData
    {
        public int Phase;
    }

    public struct AntWalkActionState : IComponentData
    {
        public int Phase;
    }
    
}
