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

namespace Abss.Motion.WithComponent
{
	

	
	public struct StreamInitialLabel : IComponentData
	{}

	public struct StreamInitialLabelFor1pos : IComponentData
	{}

	
	/// <summary>
	/// 現在キーの位置と、ストリームデータへの参照を保持する。
	/// </summary>
	public struct StreamKeyShiftData : IComponentData
	{
		public NativeSlice<KeyUnitInNative> Keys;
		
		public int      KeyIndex_Next;

	}
	/// <summary>
	/// 現在キー周辺のキーキャッシュデータ。
	/// キーがシフトしたときのみ、次のキーを読めば済むようにする。
	/// 時間は deltaTime を加算して進める。
	/// （スタート時刻と現在時刻を比較する方法だと、速度変化や休止ができないため）
	/// </summary>
	public struct StreamNearKeysCacheData : IComponentData
	{
		public float	TimeProgress;	// スタート時は 0
		public float	TimeLength;
		public float	TimeScale;
		
		public float    Time_From;
		public float    Time_To;
		public float    Time_Next;

		// 補間にかけるための現在キー周辺４つのキー
		public float4   Value_Prev;
		public float4   Value_From;	// これが現在キー
		public float4   Value_To;
		public float4   Value_Next;
	}

	/// <summary>
	/// 現在キー周辺のキーと現在時間から保管した計算結果。
	/// </summary>
	public struct StreamInterpolatedData : IComponentData
	{
		public float4	Value;
	}

	


	
	static public class StreamExtentions
	{
		
		/// <summary>
		/// キーバッファをストリーム先頭に初期化する。
		/// </summary>
		static public void InitializeKeys( [WriteOnly] ref this StreamNearKeysCacheData nearKeys, [WriteOnly] ref StreamKeyShiftData shiftInfo, float timeOffset = 0.0f )
		{
			var index0	= 0;
			var index1	= math.min( 1, shiftInfo.Keys.Length - 1 );
			var index2	= math.min( 2, shiftInfo.Keys.Length - 1 );
			
			nearKeys.Time_From = shiftInfo.Keys[ index0 ].Time.x;
			nearKeys.Time_To   = shiftInfo.Keys[ index1 ].Time.x;;
			nearKeys.Time_Next = shiftInfo.Keys[ index2 ].Time.x;

			nearKeys.Value_Prev = shiftInfo.Keys[ index0 ].Value;
			nearKeys.Value_From = shiftInfo.Keys[ index0 ].Value;
			nearKeys.Value_To	= shiftInfo.Keys[ index1 ].Value;
			nearKeys.Value_Next = shiftInfo.Keys[ index2 ].Value;

			shiftInfo.KeyIndex_Next		= index2;
			
			nearKeys.TimeProgress		= timeOffset;
		}

		/// <summary>
		/// キーバッファを次のキーに移行する。終端まで来たら、最後のキーのままでいる。
		/// </summary>
		static public void ShiftKeysIfOverKeyTime( ref StreamNearKeysCacheData nearKeys, ref StreamKeyShiftData shiftInfo )
		{
			if( nearKeys.TimeProgress < nearKeys.Time_To ) return;


			var nextIndex	= math.min( shiftInfo.KeyIndex_Next + 1, shiftInfo.Keys.Length - 1 );
			var nextKey		= shiftInfo.Keys[ nextIndex ];
			
			nearKeys.Time_From	= nearKeys.Time_To;
			nearKeys.Time_To	= nearKeys.Time_Next;
			nearKeys.Time_Next	= nextKey.Time.x;

			nearKeys.Value_Prev	= nearKeys.Value_From;
			nearKeys.Value_From	= nearKeys.Value_To;
			nearKeys.Value_To	= nearKeys.Value_Next;
			nearKeys.Value_Next	= nextKey.Value;

			shiftInfo.KeyIndex_Next	= nextIndex;
		}
		
		/// <summary>
		/// キーバッファを次のキーに移行する。ループアニメーション対応版。
		/// </summary>
		static public void ShiftKeysIfOverKeyTimeForLooping( ref StreamNearKeysCacheData nearKeys, ref StreamKeyShiftData shiftInfo )
		{
			if( nearKeys.TimeProgress < nearKeys.Time_To ) return;


			var isEndOfStream	= nearKeys.TimeProgress >= nearKeys.TimeLength;
			
			var timeOffset	= getTimeOffsetOverLength( in nearKeys );

			var nextIndex	= getNextKeyIndex( in shiftInfo );
			var nextKey		= shiftInfo.Keys[ nextIndex ];
			
			var time_from	= nearKeys.Time_To;
			var time_to		= nearKeys.Time_Next;
			var time_next	= nextKey.Time.x;

			nearKeys.Time_From	= time_from	- timeOffset;
			nearKeys.Time_To	= time_to - timeOffset;
			nearKeys.Time_Next	= time_next;

			nearKeys.Value_Prev	= nearKeys.Value_From;
			nearKeys.Value_From	= nearKeys.Value_To;
			nearKeys.Value_To	= nearKeys.Value_Next;
			nearKeys.Value_Next	= nextKey.Value;

			shiftInfo.KeyIndex_Next	= nextIndex;
			
			nearKeys.TimeProgress	-= timeOffset;

			return;

			float getTimeOffsetOverLength( in StreamNearKeysCacheData nearKeys_ )
			{
				return math.select( 0.0f, nearKeys_.TimeLength, isEndOfStream );
			}

			int getNextKeyIndex( in StreamKeyShiftData shiftInfo_ )
			{
				var iKeyLast		= shiftInfo_.Keys.Length - 1;
				var iKeyNextNext	= shiftInfo_.KeyIndex_Next + 1;

				var isEndOfKey		= iKeyNextNext > iKeyLast;

				var iWhenStayInnerKey	= math.min( iKeyNextNext, iKeyLast );
				var iWhenOverLastKey	= iKeyNextNext - math.select( 0, iKeyLast, isEndOfKey );
				
				return math.select( iWhenStayInnerKey, iWhenOverLastKey, isEndOfStream );
				// こうなってくると、素直に分岐したほうがいいんだろうかｗ←いや、はやかった
			}
		}
		
		static public void Progress( ref this StreamNearKeysCacheData nearKeys, float deltaTime )
		{
			nearKeys.TimeProgress += deltaTime * nearKeys.TimeScale;
		}

		static public float CaluclateTimeNormalized( [ReadOnly] ref this StreamNearKeysCacheData nearKeys )
		{
			var progress	= nearKeys.TimeProgress - nearKeys.Time_From;
			var length		= nearKeys.Time_To - nearKeys.Time_From;

			return math.select( progress * math.rcp( length ), 1.0f, length == 0.0f );
		}

		static public float4 Interpolate( [ReadOnly] ref this StreamNearKeysCacheData nearKeys, float normalizedTimeProgress )
		{
			
			//var s = 1;//math.sign( math.dot( nearKeys.Value_From, nearKeys.Value_To ) );

			return Utility.Interpolate(
				nearKeys.Value_Prev,
				nearKeys.Value_From,
				nearKeys.Value_To,// * s,
				nearKeys.Value_Next,// * s,
				math.saturate( normalizedTimeProgress )
			);
		}
	}

}