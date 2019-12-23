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
using Abss.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abss.Draw
{

	/// <summary>
	/// カリング用オブジェクトＡＡＢＢ
	/// </summary>
	public struct DrawTargetAabb : IComponentData
	{
		public float4	min;
		public float4	max;
	}

	/// <summary>
	/// カリング用オブジェクト球データ
	/// </summary>
	public struct DrawTargetSphere : IComponentData
	{
		public float	center;
		public float	radius;
	}

    /// <summary>
    /// 描画モデルの種類情報
    /// </summary>
    public struct DrawModelIndexData : IComponentData
    {
        public Entity ModelEntity;//
        public int ModelIndex;//
        public int BoneLength;
    }
    public struct DrawInstanceTargetWorkData : IComponentData
    {
        public int InstanceIndex;   // -1 なら描画しない
	}

    //public struct DrawBoneRelationLinkData : IComponentData
    //{
    //    public Entity BoneEntityTop;
    //}
}
