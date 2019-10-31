using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.Collections;

using Abss.Misc;

namespace Abss.Common.Extension
{

	public static class LinqExtension
	{ 

		public static IEnumerable<Tresult> Zip<T1,T2,Tresult>
			( in this (IEnumerable<T1> e1, IEnumerable<T2> e2) src, Func<T1,T2,Tresult> resultSelector )
		{
			return Enumerable.Zip( src.e1, src.e2, resultSelector );
		}
		public static IEnumerable<(T1 x, T2 y)>
			Zip<T1,T2>( in this (IEnumerable<T1> e1, IEnumerable<T2> e2) src )
		{
			return Enumerable.Zip(src.e1, src.e2, (x, y)=>(x, y) );
		}

		public static IEnumerable<Tresult> Zip<T1,T2,T3,Tresult>(
			this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3) src,
			Func<T1,T2,T3,Tresult> resultSelector
		)
		{
			//return src.e1.Zip( src.e2, (x,y)=>(x,y) ).Zip( src.e3, (xy,z)=>resultSelector(xy.x, xy.y, z) );
			var etor1 = src.e1.GetEnumerator();
			var etor2 = src.e2.GetEnumerator();
			var etor3 = src.e3.GetEnumerator();
			while( etor1.MoveNext() && etor2.MoveNext() && etor3.MoveNext() )
			{
				yield return resultSelector( etor1.Current, etor2.Current, etor3.Current );
			}
		}
		public static IEnumerable<(T1 x,T2 y,T3 z)> Zip<T1,T2,T3>
			( in this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3) src )
		{
			return src.Zip( (x,y,z)=>(x,y,z) );
		}
		
		public static IEnumerable<Tresult> Zip<T1,T2,T3,T4,Tresult>(
			this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3, IEnumerable<T4> e4) src,
			Func<T1,T2,T3,T4,Tresult> resultSelector
		)
		{
			var etor1 = src.e1.GetEnumerator();
			var etor2 = src.e2.GetEnumerator();
			var etor3 = src.e3.GetEnumerator();
			var etor4 = src.e4.GetEnumerator();
			while( etor1.MoveNext() && etor2.MoveNext() && etor3.MoveNext() && etor4.MoveNext() )
			{
				yield return resultSelector( etor1.Current, etor2.Current, etor3.Current, etor4.Current );
			}
		}
		public static IEnumerable<(T1 x,T2 y,T3 z, T4 w)> Zip<T1,T2,T3,T4>
			( in this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3, IEnumerable<T4> e4) src )
		{
			return src.Zip( (x,y,z,w)=>(x,y,z,w) );
		}

		
		public static T[][][] ToArrayRecursive3<T>
			( this IEnumerable<IEnumerable<IEnumerable<T>>> src )
		{
			return src.Select( x => x.ToArrayRecursive2() ).ToArray();
		}
		public static T[][] ToArrayRecursive2<T>( this IEnumerable<IEnumerable<T>> src )
		{
			return src.Select( x => x.ToArray() ).ToArray();
		}
		//public static List<T> ToListRecursive<T>( this IEnumerable<T> src )
		//{
		//	return src.ToList();
		//}

		public static IEnumerable<T> EmptyIfNull<T>( this IEnumerable<T> src )
		{
			return src ?? Enumerable.Empty<T>();
		}

		public static IEnumerable<T> SelectMany<T>( this IEnumerable<IEnumerable<T>> src )
		{
			return src.SelectMany( x => x );
		}
		
		//public static IEnumerable<T> Prepend<T>( this IEnumerable<T> src, T element )
		//{
		//	return Enumerable.Repeat( element, 1 ).Concat( src );
		//}
		//public static IEnumerable<T> Append<T>( this IEnumerable<T> src, T element )
		//{
		//	return src.Concat( Enumerable.Repeat( element, 1 ) );
		//}
        
	}

    // あとで統合しよう
    /*
	public static class ConversionExtension
	{
		// As はイメージそのままで型表現だけ変更、To は変換されるんだけど、名前似てるからバグを生みそう…。
		// → 考えたら To は new Vector4() でええやんけ

		public static float AsFloat( this int value )
		{
			return new FloatInt { IntValue = value }.FloatValue;
		}
		public static int AsInt( this float value )
		{
			return new FloatInt { FloatValue = value }.IntValue;
		}
		public static Vector4 AsVector4( in this (int x, int y, int z, int w) v )
		{
			return new Vector4( v.x.AsFloat(), v.y.AsFloat(), v.z.AsFloat(), v.w.AsFloat() );
		}
		//public static Vector4 ToVector4( in this (int x, int y, int z, int w) v )
		//{
		//	return new Vector4( v.x, v.y, v.z, v.w );
		//}
		//public static Vector4 ToVector4( in this (float x, float y, float z, float w) v )
		//{
		//	return new Vector4( v.x, v.y, v.z, v.w );
		//}

		public static Vector4 ToVector4( this int[] value )
		{
			return new Vector4( value[0], value[1], value[2], value[3] );
		}
		public static Vector4 ToVector4( this int[] value, int offset )
		{
			return new Vector4( value[offset+0], value[offset+1], value[offset+2], value[offset+3] );
		}
		public static Vector4 ToVector4( this float[] value )
		{
			return new Vector4( value[0], value[1], value[2], value[3] );
		}
		public static Vector4 ToVector4( this float[] value, int offset )
		{
			return new Vector4( value[offset+0], value[offset+1], value[offset+2], value[offset+3] );
		}

	
		[StructLayout(LayoutKind.Explicit)]
		struct FloatInt
		{
			[FieldOffset(0)] public float FloatValue;
			[FieldOffset(0)] public int IntValue;
		}
	}
    */

	public static class FuncExtension
	{ 
		/// <summary>
		/// 左→右に関数をつなぎ、値をパイプのように流していく。
		/// </summary>
		public static Tdst To<Tsrc,Tdst>( this Tsrc src, Func<Tsrc,Tdst> nextfunction )
		{
			return nextfunction( src );
		}
		/// <summary>
		/// 左→右に関数をつなぎ、値をパイプのように流していく。流す値は、２値のタプル。
		/// </summary>
		public static Tdst To<Tsrc0,Tsrc1,Tdst>( this (Tsrc0 v0, Tsrc1 v1) src, Func<Tsrc0,Tsrc1,Tdst> nextfunction )
		{
			return nextfunction( src.v0, src.v1 );
		}
		/// <summary>
		/// 左→右に関数をつなぎ、値をパイプのように流していく。流す値は、３値のタプル。
		/// </summary>
		public static Tdst To<Tsrc0,Tsrc1,Tsrc2,Tdst>
			( this (Tsrc0 v0, Tsrc1 v1, Tsrc2 v2) src, Func<Tsrc0,Tsrc1,Tsrc2,Tdst> nextfunction )
		{
			return nextfunction( src.v0, src.v1, src.v2 );
		}
		/// <summary>
		/// 左→右に関数をつなぎ、値をパイプのように流していく。
		/// </summary>
		public static void To<Tsrc>( this Tsrc src, Action<Tsrc> nextfunction )
		{
			nextfunction( src );
		}
	}

	public static class UnityObjectExtension
	{
        /// <summary>
        /// unity オブジェクトのニセ null を、真の null に変換する。
        /// </summary>
		public static T As<T>( this T obj ) where T:UnityEngine.Object
		{
			return obj != null ? obj : null;
		}

		public static void DestroyComponentIfExists<T>( this GameObject gameObject ) where T:Component
		{
			var c = gameObject.GetComponent<T>();
			if( c != null ) UnityEngine.Object.Destroy( c );
		}
	}

	public static class VectorExtension
	{
		public static bool IsMuinusScale( in this Matrix4x4 mt )
		{
			var up = Vector3.Cross( mt.GetRow( 0 ), mt.GetRow( 2 ) );
			return Vector3.Dot( up, mt.GetRow( 1 ) ) > 0.0f;
			//var scl = tf.lossyScale;
			//return scl.x * scl.y * scl.z < 0.0f;
		}
	}

    public static class EntitiesExtension
    {
        public static void SetComponentData<T>
            ( this EntityManager em, IEnumerable<Entity> entities, IEnumerable<T> components )
            where T : struct, IComponentData
        {
            foreach( var x in (entities,components).Zip() )
            {
                em.SetComponentData( x.x, x.y );
            }
        }

        public static void AddComponentData<T>
            ( this EntityManager em, IEnumerable<Entity> entities, IEnumerable<T> components )
            where T : struct, IComponentData
        {
            foreach( var x in (entities, components).Zip() )
            {
                em.AddComponentData( x.x, x.y );
            }
        }

        public static void SetLinkedEntityGroup
            ( this EntityManager em, Entity entity, IEnumerable<Entity> children )
        {
            // リンク一時配列生成
            var linkedEntityGroup = children
                .Prepend( entity )
                .Select( ent => new LinkedEntityGroup { Value = ent } )
                .ToNativeArray( Allocator.Temp );

            // バッファに追加
            var buf = em.AddBuffer<LinkedEntityGroup>( entity );
            buf.AddRange( linkedEntityGroup );

            // 一時配列破棄
            linkedEntityGroup.Dispose();
        }
    }
}
