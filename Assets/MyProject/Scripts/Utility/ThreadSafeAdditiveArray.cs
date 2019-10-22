using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

// わかりにくい、統一感がない、そのせいか使いにくい
namespace Abss.Misc
{

	/// <summary>
	/// [WriteOnly] NativeArray にジョブからアイテムを追加していける。
	/// ＣＡＳ命令でインデックスを得ているが、スレッドごとに領域を作るのとどちらが良いのだろうか？
	/// </summary>
	public struct ThreadSafeAdditiveArray<T, Tallocator> : IDisposable
		where Tallocator:IAllocatorLabel, new()
		where T:struct
	{

		[WriteOnly][NativeDisableParallelForRestriction]
		public NativeArray<T>					Values;

		public ThreadSafeCounter<Tallocator>	Counter;

		public readonly int	Stride;
		

		public ThreadSafeAdditiveArray( int capacity, int stride = 1 )
		{
			Values	= new NativeArray<T>( capacity * stride, new Tallocator().Label, NativeArrayOptions.UninitializedMemory );

			Counter	= new ThreadSafeCounter<Tallocator>( 0 );

			Stride	= stride;
		}

		public void Dispose()
		{
			if( !Values.IsCreated ) return;
			Values.Dispose();
			Counter.Dispose();
		}

		public void Add( ref T value )
		{
			var i = Counter.GetSerial();

			Values[i * Stride]	= value;
		}

	}
	

	/// <summary>
	/// 
	/// </summary>
	public struct ThreadSafeAdditiveArrayTempJob<T>
		where T:struct
	{

		NativeArray<T>				values;

		ThreadSafeCounterTempJob	counter;

		readonly int				stride;


		public int Count => counter.Count;
		
		public ThreadSafeAdditiveArrayTempJob( int capacity, int stride = 1 )
		{
			values	= new NativeArray<T>( capacity * stride, Allocator.TempJob, NativeArrayOptions.UninitializedMemory );

			counter	= new ThreadSafeCounterTempJob( 0 );

			this.stride	= stride;
		}


		/// <summary>
		/// 書込専用構造体に詰め替えて返す。[WriteOnly] と [NativeDisableParallelForRestriction] が付与される。
		/// </summary>
		public AdditiveArrayWriteOnly ToWriteOnly()
		{
			return new AdditiveArrayWriteOnly( ref values, ref counter, stride );
		}
		public struct AdditiveArrayWriteOnly
		{
			[NativeDisableParallelForRestriction][WriteOnly]
			NativeArray<T>				values;
			ThreadSafeCounterTempJob	counter;
			readonly int				stride;

			internal AdditiveArrayWriteOnly( ref NativeArray<T> value, ref ThreadSafeCounterTempJob counter, int strid )
			{
				this.values		= value;
				this.counter	= counter;
				this.stride		= strid;
			}

			public void Add( ref T value )
			{
				var i = counter.GetSerial();

				values[ i * stride ] = value;
			}
		}

		/// <summary>
		/// 読取後破棄用の構造体に詰め替えて返す。[ReadOnly] と [DeallocateOnJobCompletion] が付与される。
		/// </summary>
		public DiscardableArrayReadOnly ToReadOnlyDiscard()
		{
			return new DiscardableArrayReadOnly( ref values, ref counter );
		}
		public struct DiscardableArrayReadOnly
		{
			[DeallocateOnJobCompletion][ReadOnly]
			public NativeArray<T>						Values;
			ThreadSafeCounterTempJob.DiscardableCounter	counter;
			
			public int Count => counter.Count;

			internal DiscardableArrayReadOnly( ref NativeArray<T> values, ref ThreadSafeCounterTempJob counter )
			{
				this.Values		= values;
				this.counter	= counter.ToReadOnlyDiscad() ;
			}
		}

	}


	static class AdditiveArrayJobExtension
	{

		//static JobHandle Schedule<T, U>( ref this T job, NativeList<U> list, JobHandle inputDeps )
		//	where T:struct, IJobParallelFor
		//	where U:struct
		//{
		//	return job.Schedule( list, 0, inputDeps );
		//}


	}
}
