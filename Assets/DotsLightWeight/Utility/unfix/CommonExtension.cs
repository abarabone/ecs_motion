﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Collections;

using Abarabone.Misc;

namespace Abarabone.Common.Extension
{


	public static class ArithmetichExtension
    {

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int PostAdd(ref this int i, int value) { var res = i; i += value; return res; }

	}



	public static class LinqExtension
	{

		/// <summary>
		/// 
		/// </summary>
		static public Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) =>
			Task.WhenAll(tasks);


		public static IEnumerable<Tresult> Zip<T1,T2,Tresult>
			( in this (IEnumerable<T1> e1, IEnumerable<T2> e2) src, Func<T1,T2,Tresult> resultSelector )
		{
			return Enumerable.Zip( src.e1, src.e2, resultSelector );
		}
		public static IEnumerable<(T1 src0, T2 src1)>
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
		public static IEnumerable<(T1 src0,T2 src1,T3 src2)> Zip<T1,T2,T3>
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
		public static IEnumerable<(T1 src0,T2 src1,T3 src2, T4 src3)> Zip<T1,T2,T3,T4>
			( in this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3, IEnumerable<T4> e4) src )
		{
			return src.Zip( (x,y,z,w)=>(x,y,z,w) );
		}

		public static IEnumerable<Tresult> Zip<T1, T2, T3, T4, T5, Tresult>(
			this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3, IEnumerable<T4> e4, IEnumerable<T5> e5) src,
			Func<T1, T2, T3, T4, T5, Tresult> resultSelector
		)
		{
			var etor1 = src.e1.GetEnumerator();
			var etor2 = src.e2.GetEnumerator();
			var etor3 = src.e3.GetEnumerator();
			var etor4 = src.e4.GetEnumerator();
			var etor5 = src.e5.GetEnumerator();
			while (etor1.MoveNext() && etor2.MoveNext() && etor3.MoveNext() && etor4.MoveNext() && etor5.MoveNext())
			{
				yield return resultSelector(etor1.Current, etor2.Current, etor3.Current, etor4.Current, etor5.Current);
			}
		}
		public static IEnumerable<(T1 src0, T2 src1, T3 src2, T4 src3, T5 src4)> Zip<T1, T2, T3, T4, T5>
			(in this (IEnumerable<T1> e1, IEnumerable<T2> e2, IEnumerable<T3> e3, IEnumerable<T4> e4, IEnumerable<T5> e5) src)
		{
			return src.Zip((x, y, z, w, a) => (x, y, z, w, a));
		}


		public static IEnumerable<T>
            Concat<T>( in this (IEnumerable<T> e1, IEnumerable<T> e2) src )
        {
            return Enumerable.Concat( src.e1, src.e2 );
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


		public static IEnumerable<(T src, int i)> WithIndex<T>(this IEnumerable<T> src) =>
			src.Select((x, i) => (x, i));


		public static IEnumerable<T> SelectMany<T>( this IEnumerable<IEnumerable<T>> src )
		{
			return src.SelectMany( x => x );
		}

		public static Dictionary<Tkey, Tvalue> ToDictionary<Tkey, Tvalue>
			(this IEnumerable<(Tkey k, Tvalue v)> src)
		=>
			src.ToDictionary(x => x.k, x => x.v);
		
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

		public static T GetComponentOrNull<T>(this GameObject gameObject) where T : Component =>
			gameObject.TryGetComponent<T>(out var c) ? c : null;
		public static T GetComponentOrNull<T>(this Transform tf) where T : Component =>
			tf.TryGetComponent<T>(out var c) ? c : null;
	}

	public static class VectorExtension
	{
		public static bool IsMuinusScale( this Matrix4x4 mt )
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
                em.SetComponentData( x.src0, x.src1 );
            }
        }
        public static void AddComponentData<T>
            ( this EntityManager em, IEnumerable<Entity> entities, IEnumerable<T> components )
            where T : struct, IComponentData
        {
            foreach( var x in (entities, components).Zip() )
            {
                em.AddComponentData( x.src0, x.src1 );
            }
        }

        public static void AddComponents
            ( this EntityManager em, IEnumerable<Entity> entities, ComponentTypes types )
        {
            foreach( var x in entities )
            {
                em.AddComponents( x, types );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetComponentData<T>
            ( this EntityManager em, IEnumerable<Entity> entities, T component )
            where T:struct,IComponentData
        {
            var q = Enumerable.Repeat( component, entities.getLength() );
            em.SetComponentData( entities, q );
        }
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void AddComponentData<T>
            ( this EntityManager em, IEnumerable<Entity> entities, T component )
            where T : struct, IComponentData
        {
            var q = Enumerable.Repeat( component, entities.getLength() );
            em.AddComponentData( entities, q );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static int getLength<T>( this IEnumerable<T> src ) where T:struct
        {
            switch( src )
            {
                case NativeArray<T> na: return na.Length;
                case NativeList<T> nl: return nl.Length;
                default: return src.Count();
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

    static public class GameObjectConversionExtension
    {

        /// <summary>
        /// アーキタイプを指定して、追加のエンティティを生成する。
        /// </summary>
        static public Entity CreateAdditionalEntity<T>
            ( this GameObjectConversionSystem gcs, GameObject mainGameObject )
            where T:IComponentData
        {
            var ent = gcs.CreateAdditionalEntity( mainGameObject );

            gcs.DstEntityManager.AddComponent<T>( ent );

            return ent;
        }
        /// <summary>
        /// アーキタイプを指定して、追加のエンティティを生成する。
        /// </summary>
        static public Entity CreateAdditionalEntity
            ( this GameObjectConversionSystem gcs, GameObject mainGameObject, EntityArchetype archetype )
        {
            var ent = gcs.CreateAdditionalEntity( mainGameObject );

            gcs.DstEntityManager.SetArchetype( ent, archetype );

            return ent;
        }
        /// <summary>
        /// アーキタイプを指定して、追加のエンティティを複数生成。配列として返す。
        /// </summary>
        static public Entity[] CreateAdditionalEntities
            ( this GameObjectConversionSystem gcs, GameObject mainGameObject, EntityArchetype archetype, int length )
        {
            return Enumerable.Range(0, length)
                .Select( i => gcs.CreateAdditionalEntity( mainGameObject, archetype ) )
                .ToArray();
        }
        /// <summary>
        /// アーキタイプを指定して、追加のエンティティを複数生成。NativeArray として返す。
        /// </summary>
        static public NativeArray<Entity> CreateAdditionalEntities<TallocatorLabel>
            ( this GameObjectConversionSystem gcs, GameObject mainGameObject, EntityArchetype archetype, int length )
            where TallocatorLabel : IAllocatorLabel, new()
        {
            var ents = new NativeArray<Entity>( length, new TallocatorLabel().Label );

            for( var i=0; i<length; i++ )
            {
                ents[i] = gcs.CreateAdditionalEntity( mainGameObject, archetype );
            }

            return ents;
        }

    }
}
