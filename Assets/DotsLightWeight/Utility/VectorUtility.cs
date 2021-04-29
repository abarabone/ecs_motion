using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Collections;
using Unity.Mathematics;

namespace DotsLite.Utilities
{

    /// <summary>
    /// ï˚å¸Ç∆í∑Ç≥ÇÅA4 floats Ç≈ä«óùÇ∑ÇÈÅB
    /// </summary>
    public struct DirectionAndLength
    {
        public float4 Value;

        public float3 Ray
		{
			get => this.Direction * this.Length;
		}
        public float3 Direction
		{
			get => Value.xyz;
			set => this.Value = new float4(value, this.Value.w);
		}
        public float Length
		{
			get => Value.w;
			set => this.Value.w = value;
		}

		public static implicit operator DirectionAndLength (float4 src) => new DirectionAndLength { Value = src };
    }

    public static class VectorUtility
    {

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public float4 As_float4(this Quaternion q)
		//{
		//	return q.As_float4();//new float4(q.x, q.y, q.z, q.w);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public float4 As_float4(this Vector4 v)
		//{
		//	return v;//new float4(v.x, v.y, v.z, v.w);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public float3 As_float3(this Vector3 v)
		//{
		//	return v;//new float3(v.x, v.y, v.z);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public float3 As_float3(this Vector4 v)
		//{
		//	return v.As_float4().xyz;//new float3(v.x, v.y, v.z);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public quaternion As_quaternion(this Vector4 v)
		//{
		//	return new quaternion(v);//new quaternion(v.x, v.y, v.z, v.w);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public quaternion As_quaternion(this Quaternion q)
		//{
		//	return q;//new quaternion(v.x, v.y, v.z, v.w);
		//}


		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public float4 As_float4(this float3 v, float w = 0.0f)
		//{
		//	return new float4(v, w);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public float3 As_float3(this float4 v)
		//{
		//	return v.xyz;//new float3(v.x, v.y, v.z);
		//}
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//static public quaternion As_quaternion(this float4 v)
		//{
		//	return new quaternion(v);//new quaternion(v.x, v.y, v.z, v.w);
		//}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float4 As_float4(this Quaternion q)
		{
			return new float4(q.x, q.y, q.z, q.w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float4 As_float4(this Vector4 v)
		{
			return new float4(v.x, v.y, v.z, v.w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float3 As_float3(this Vector3 v)
		{
			return new float3(v.x, v.y, v.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float3 As_float3(this Vector4 v)
		{
			return new float3(v.x, v.y, v.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public quaternion As_quaternion(this Vector4 v)
		{
			return new quaternion(v.x, v.y, v.z, v.w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public quaternion As_quaternion(this Quaternion q)
		{
			return new quaternion(q.x, q.y, q.z, q.w);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float4 As_float4(this float3 v, float w = 0.0f)
		{
			return new float4(v, w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float3 As_float3(this float4 v)
		{
			return new float3(v.x, v.y, v.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public quaternion As_quaternion(this float4 v)
		{
			return new quaternion(v.x, v.y, v.z, v.w);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float4 Interpolate(float4 v0, float4 v1, float4 v2, float4 v3, float t)
        {
            //return Quaternion.Slerp( new quaternion(v1), new quaternion(v2), Mathf.Clamp01(t) ).ToFloat4();
            return v1 + 0.5f * t * ((v2 - v0) + ((2.0f * v0 - 5.0f * v1 + 4.0f * v2 - v3) + (-v0 + 3.0f * v1 - 3.0f * v2 + v3) * t) * t);
        }

    }
}