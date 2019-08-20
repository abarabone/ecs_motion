using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
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
	public struct DrawModelInfo : IComponentData
	{
		public int	modelIndex;
	}

	
}
