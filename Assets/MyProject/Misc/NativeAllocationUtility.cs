using System.Threading;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Abss.Misc
{
    
	/// <summary>
	/// Allocator 情報を型に持たせるために使用する。
	/// </summary>
	public interface IAllocatorLabel
	{
		Allocator Label { get; }
	}
	public struct Persistent : IAllocatorLabel
	{
		public Allocator Label { get => Allocator.Persistent; }
	}
	public struct Temp : IAllocatorLabel
	{
		public Allocator Label { get => Allocator.Temp; }
	}
	// TempJob は別の仕組みを作るので使用しない
	
	

	public static class NativeArrayExtention
	{
		
		static public NativeArray<T> ToNativeArray<T>( this T[] array, Allocator allocator ) where T : struct
		{
			return new NativeArray<T>( array, allocator );
		}

		static public NativeArray<T> ToNativeArray<T>( this IEnumerable<T> e, Allocator allocator ) where T : struct
		{
			return new NativeArray<T>( e.ToArray(), allocator );
		}

	}
	
	public static class NativeListExtension
	{

		/// <summary>
		/// パラレル処理可能な NativeList を返す。
		/// </summary>
		public static NativeListConcurrent<T> ToConcurrent<T>( ref this NativeList<T> list )
			where T:struct
		{
			return new NativeListConcurrent<T>( ref list );
		}


		/// <summary>
		/// Job に渡すために、TempJob な NativeList を自動開放可能な構造体に詰め込む。
		/// Defer 系の Schedule() と併用する。
		/// </summary>
		public static NativeListDeferDiscardableReadOnly<T> ToDeferDiscardableReadOnly<T>( ref this NativeList<T> list )
			where T:struct
		{
			return new NativeListDeferDiscardableReadOnly<T>( in list );
		}

	}

	
	// NativeListImpl で定義されているが、private なのでここで定義する。定まっていない仕様の類だろう。
	internal unsafe struct NativeListData
	{
		public void	*buffer;
		public int	length;
		public int	capacity;
	}
	
	/// <summary>
	/// パラレル処理可能な NativeList 。
	/// </summary>
	public unsafe struct NativeListConcurrent<T> where T:struct
	{
		[NativeDisableUnsafePtrRestriction]
		private void	*pElements;
		[NativeDisableUnsafePtrRestriction]
		private int		*pLength;

		public NativeListConcurrent( ref NativeList<T> origin )
		{
			var pListData		= NativeListUnsafeUtility.GetInternalListDataPtrUnchecked( ref origin );
			ref var listData	= ref Unsafe.AsRef<NativeListData>( pListData );

			pElements	= listData.buffer;
			pLength		= (int*)UnsafeUtility.AddressOf( ref listData.length );
		}

		/// <summary>
		/// 要素の追加。ただし、capacity は考慮しない。
		/// </summary>
		public void Add( in T element )
		{
			// capacity を超えた場合は、WriteArrayElement のエラーに任せる（ハングする？？）
			var i	= Interlocked.Increment( ref *pLength ) - 1;

			// NativeListImpl の set で使用されている構文と同じ
			UnsafeUtility.WriteArrayElement( pElements, i, element );
		}
	}
	
	/// <summary>
	/// Job で開放できる NativeList を詰め込む構造体。Elements[i] を通して要素にアクセスできる。
	/// </summary>
	public struct NativeListDeferDiscardableReadOnly<T> where T:struct
	{
		[ReadOnly]
		NativeList<T>	origin;
		[NativeDisableParallelForRestriction][ReadOnly]
		public NativeArray<T>	Elements;


		public NativeList<T> OriginList => this.origin;

		public NativeListDeferDiscardableReadOnly( in NativeList<T> origin )
		{
			this.origin		= origin;
			this.Elements	= origin.ToDeferredJobArray();
		}
	}


	/// <summary>
	/// NativeArray を等分割して管理する。
	/// セグメントへのアクセスは、内部で作成した NativeSlice を経由して行われる。
	/// </summary>
	public unsafe struct SegmentalNativeArray<T> : System.IDisposable
		where T:struct
	{
		[NativeDisableParallelForRestriction]
		readonly NativeArray<T>	pool;			// バッファ

		int	lengthInSegment;


		//[NativeDisableParallelForRestriction]
		//public readonly NativeArray<NativeSlice<T>>	segments;	// セグメント配列
		// コンストラクタで生成しようと思ったが、余計なメモリ管理／アクセスが発生する（と思われる）のでやめた。


		public ref T Element( int segmentIndex, int elementIndex )
		{
			var pPool = pool.GetUnsafePtr();
			return ref UnsafeUtilityEx.ArrayElementAsRef<T>( pPool, segmentIndex * lengthInSegment + elementIndex );
		}

		//public SegmentalNativeArrayWriteOnly<T> ToWriteOnly()
		//{
		//	return new SegmentalNativeArrayWriteOnly<T>( this );
		//}
		//public SegmentalNativeArrayReadOnly<T> ToReadOnly()
		//{
		//	return new SegmentalNativeArrayReadOnly<T>( this );
		//}

		public void Dispose()
		{
			pool.Dispose();
		}

		public SegmentalNativeArray( int segmentCount, int lengthInSegment, Allocator allocator )
		{
			this.pool				= new NativeArray<T>( lengthInSegment * segmentCount, allocator );
			this.lengthInSegment	= lengthInSegment;
		}
	}

	//public unsafe struct SegmentalNativeArrayWriteOnly<T>
	//	where T:struct
	//{
	//	[NativeDisableParallelForRestriction][WriteOnly]
	//	public readonly NativeArray<NativeSlice<T>>	segments;

	//	public SegmentalNativeArrayWriteOnly( SegmentalNativeArray<T> src )
	//	{
	//		this.segments = src.segments;
	//	}
	//}

	//public unsafe struct SegmentalNativeArrayReadOnly<T>
	//	where T:struct
	//{
	//	[NativeDisableParallelForRestriction][ReadOnly]
	//	public readonly NativeArray<NativeSlice<T>>	segments;
		
	//	public SegmentalNativeArrayReadOnly( SegmentalNativeArray<T> src )
	//	{
	//		this.segments = src.segments;
	//	}
	//}
}
