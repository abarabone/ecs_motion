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


namespace DotsLite.Geometry
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

		static public bool IsMuinusScale(this float4x4 mt)
		{
			var mtinv = math.transpose(mt);
			var up = math.cross(mtinv.c0.xyz, mtinv.c2.xyz);
			return Vector3.Dot(up, mtinv.c1.xyz) > 0.0f;
		}

		static public IEnumerable<IEnumerable<T>> AsTriangle<T>(this IEnumerable<T> indecies_)
			where T : struct
		{
			using var e = indecies_.GetEnumerator();

			while (e.MoveNext())
			{
				yield return tri_();

				IEnumerable<T> tri_()
				{
					yield return e.Current; e.MoveNext();
					//if (!e.MoveNext()) yield break;
					yield return e.Current; e.MoveNext();
					//if (!e.MoveNext()) yield break;
					yield return e.Current;
				}
			}
		}


		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static public Entity AddComponentData<T>( this Entity ent, EntityManager entityManager, T componentData )
			where T:struct, IComponentData
		{
			entityManager.AddComponentData( ent, componentData );

			return ent;
		}
	}


}
