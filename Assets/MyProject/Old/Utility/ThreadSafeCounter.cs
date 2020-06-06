using System.Threading;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Abss.Misc
{

	/// <summary>
	/// スレッドセーフな採番。
	/// コンストラクタに初期値を与えると、採番用共有メモリを確保する。
	/// 初期値を与えるコンストラクタのみ、領域を確保する。
	/// </summary>
	public unsafe struct ThreadSafeCounter<Tallocator> : IDisposable
		where Tallocator:IAllocatorLabel, new()
	{

		[NativeDisableUnsafePtrRestriction]
		int* pCounter;
		
		public int Count { get => *pCounter; }


		public ThreadSafeCounter( int initValue )
		{
            var size = UnsafeUtility.SizeOf<int>();
            var align = UnsafeUtility.AlignOf<int>();
			pCounter = (int*)UnsafeUtility.Malloc( size, align, new Tallocator().Label );
			
			*pCounter = initValue;
		}

		public void Dispose()
		{
			if( pCounter == null ) return;

			UnsafeUtility.Free( pCounter, new Tallocator().Label );

			pCounter = null;
		}


		public void Reset()
		{
			if( pCounter == null ) return;

			*pCounter = 0;
		}

		public int GetSerial()
		{
			return Interlocked.Increment( ref *pCounter ) - 1;
		}
	}


	/// <summary>
	/// スレッドセーフな採番。Job に渡して使用する。
	/// コンストラクタに初期値を与えると、採番用共有メモリを確保する。
	/// メモリ領域は、読取後破棄として渡したジョブが終了すると開放される。
	/// 領域は、構造体のコピーでは確保されない。
	/// </summary>
	public struct ThreadSafeCounterTempJob
	{
		
		[NativeDisableParallelForRestriction]
		NativeArray<int>	counter;
		
		public int Count => counter[0];


		public ThreadSafeCounterTempJob( int initValue )
		{
			//pCounter = (int*)UnsafeUtility.Malloc( UnsafeUtility.SizeOf<int>(), 4, Allocator.TempJob );
			counter = new NativeArray<int>( 1, Allocator.TempJob, NativeArrayOptions.UninitializedMemory );
			
			counter[0] = initValue;
		}
		

		/// <summary>
		/// ロックレスでスレッドセーフに連番を取得する。
		/// </summary>
		public unsafe int GetSerial()
		{
			var pCounter = (int*)NativeArrayUnsafeUtility.GetUnsafePtr( counter );

			return Interlocked.Increment( ref *pCounter ) - 1;
		}


		/// <summary>
		/// 読取後破棄用の構造体に詰め替えて返す。[ReadOnly] と [DeallocateOnJobCompletion] が付与される。
		/// </summary>
		public DiscardableCounter ToReadOnlyDiscad()
		{
			return new DiscardableCounter( ref counter );
		}
		public struct DiscardableCounter
		{
			[DeallocateOnJobCompletion][ReadOnly]
			NativeArray<int>	counter;

			public int Count => counter[0];

			internal DiscardableCounter( ref NativeArray<int> counter )
			{
				this.counter = counter;
			}
		}

	}

}





