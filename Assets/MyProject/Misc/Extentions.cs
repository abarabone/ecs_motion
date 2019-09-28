using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections;
using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using System.Runtime.CompilerServices;


namespace Abss.Geometry
{

	static public class GeometryExtentions
	{
		
	    // This is the ugly bit, necessary until Graphics.DrawMeshInstanced supports NativeArrays pulling the data in from a job.
        public unsafe static Matrix4x4[] CopyToArray( this NativeSlice<Matrix4x4> src, Matrix4x4[] dst )// where T:struct
        {
			fixed( Matrix4x4* p = dst )
            {
				UnityEngine.Assertions.Assert.IsTrue( UnsafeUtility.IsBlittable<Matrix4x4>() );
	            var s = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<Matrix4x4>( p, UnsafeUtility.SizeOf<Matrix4x4>(), src.Length );
	            #if ENABLE_UNITY_COLLECTIONS_CHECKS
	            NativeSliceUnsafeUtility.SetAtomicSafetyHandle( ref s, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle() );
	            #endif
                s.CopyFrom( src );
            }
			return dst;
        }
	    // This is the ugly bit, necessary until Graphics.DrawMeshInstanced supports NativeArrays pulling the data in from a job.
        public unsafe static Vector4[] CopyToArray( this NativeSlice<Vector4> src, Vector4[] dst )// where T:struct
        {
			fixed( Vector4* p = dst )
            {
				UnityEngine.Assertions.Assert.IsTrue( UnsafeUtility.IsBlittable<Vector4>() );
	            var s = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<Vector4>( p, UnsafeUtility.SizeOf<Vector4>(), src.Length );
	            #if ENABLE_UNITY_COLLECTIONS_CHECKS
	            NativeSliceUnsafeUtility.SetAtomicSafetyHandle( ref s, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle() );
	            #endif
                s.CopyFrom( src );
            }
			return dst;
        }

		/*
		static public Vector4 ToVector4( this Quaternion q )
		{
			return new Vector4( q.x, q.y, q.z, q.w );
		}
		
		static public Quaternion ToQuaternion( this float4 v )
		{
			return new Quaternion( v.x, v.y, v.z, v.w );
		}
		static public Vector4 ToPosition( this Vector4 v )
		{
			return new Vector4( v.x, v.y, v.z, 0.0f );
		}
		*/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float4 As_float4( this Quaternion q )
		{
			return new float4( q.x, q.y, q.z, q.w );
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float4 As_float4( this Vector4 v )
		{
			return new float4( v.x, v.y, v.z, v.w );
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float3 As_float3( this Vector3 v )
		{
			return new float3( v.x, v.y, v.z );
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float3 As_float3( this Vector4 v )
		{
			return new float3( v.x, v.y, v.z );
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public quaternion As_quaternion( this Vector4 v )
		{
			return new quaternion( v.x, v.y, v.z, v.w );
		}
		
		
		//static public Vector4 ToVector4( this float3 v, float w = 0.0f )
		//{
		//	return new Vector4( v.x, v.y, v.z, w );
		//}
		//static public Vector4 ToVector4( this quaternion q )
		//{
		//	var v = q.value;
		//	return new Vector4( v.x, v.y, v.z, v.w );
		//}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float4 As_float4( this float3 v, float w = 0.0f )
		{
			return new float4( v, w );
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public float3 As_float3( this float4 v )
		{
			return new float3( v.x, v.y, v.z );
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public quaternion As_quaternion( this float4 v )
		{
			return new quaternion( v.x, v.y, v.z, v.w );
		}

		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public Entity AddComponentData<T>( this Entity ent, EntityManager entityManager, T componentData )
			where T:struct, IComponentData
		{
			entityManager.AddComponentData( ent, componentData );

			return ent;
		}
	}

	static class Utility
	{
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float4 Interpolate( float4 v0, float4 v1, float4 v2, float4 v3, float t )
		{
			//return Quaternion.Slerp( new quaternion(v1), new quaternion(v2), Mathf.Clamp01(t) ).ToFloat4();
			return v1 + 0.5f * t * ( ( v2 - v0 ) + ( ( 2.0f * v0 - 5.0f * v1 + 4.0f * v2 - v3 ) + ( -v0 + 3.0f * v1 - 3.0f * v2 + v3 ) * t ) * t );
		}
        
	}

}
