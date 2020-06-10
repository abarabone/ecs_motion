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
using Abarabone.Geometry;
using System.Runtime.InteropServices;
using System;

using Abarabone.Utilities;

namespace Abarabone.Draw
{

    public struct DrawInstanceEntity :
        ITypedEntity<
            DrawInstanceModeLinkData,
            DrawInstanceTargetWorkData
        >
    {
        public Entity Entity { get; set; }

        static public implicit operator DrawInstanceEntity( Entity ent )
            => new DrawInstanceEntity { Entity = ent };
    }



    /// <summary>
    /// 描画モデルの種類情報
    /// </summary>
    public struct DrawInstanceModeLinkData : IComponentData
    {
        public Entity DrawModelEntity;
    }
    public struct DrawInstanceTargetWorkData : IComponentData
    {
        public int DrawInstanceId;   // -1 なら描画しない
    }


    /// <summary>
    /// カリング用オブジェクトＡＡＢＢ
    /// </summary>
    public struct DrawInstanceTargetAabb : IComponentData
    {
        public float4 min;
        public float4 max;
    }

    /// <summary>
    /// カリング用オブジェクト球データ
    /// </summary>
    public struct DrawInstanceTargetSphere : IComponentData
    {
        public float center;
        public float radius;
    }




}
