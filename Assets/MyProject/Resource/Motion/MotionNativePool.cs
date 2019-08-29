using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections;
using System;
using System.IO;

using Abss.Geometry;
using Abss.Misc;


namespace Abss.Motion
{
    using Utilities;


	public enum KeyStreamSection
	{
		positions,
		rotations,
		scales,
		length
	}


	// モーションデータ（コンポーネント用）----------------------

	// コンポーネントデータはネイティブ形式でないと扱えないため、マネージドからコンバートしなくてはならない。
	// （この形式のままマネージドに変換してアセットにしておく、という手もあるかも…。ただ、内容を見ても意味不明ではあるが。）

	public struct MotionUnitInNative
	{
		public float	TimeLength;
	}

	public struct StreamSliceInNative
	{
		public NativeSlice<KeyUnitInNative>	Keys;
	}

	public struct KeyUnitInNative
	{
		public float4	Time;
		public float4	Value;
	//	public float4	Value2;//
	}

	public struct MotionPoolInNative
	{
		public NativeArray<MotionUnitInNative>	Motions;
		public NativeArray<StreamSliceInNative>	StreamSlices;
		public NativeArray<KeyUnitInNative>		Keys;
		public NativeArray<int>					BoneParents;
	}


	/// <summary>
	/// モーションデータ（コンポーネント用に確保）クラス
	/// 　ＦＢＸアセットに含まれるクリップすべてが格納されている（追加できるようにしてもいいな）
	/// </summary>
	public class MotionDataInNative : IDisposable
	{

		internal MotionPoolInNative	pool;
	
		internal int	boneLength;

	
		/// <summary>
		/// ストリーム格納プールを取得する。
		/// </summary>
		public NativeArray<StreamSliceInNative> StreamPool => pool.StreamSlices;

		/// <summary>
		/// ボーン数を取得する。
		/// </summary>
		public int BoneLength => boneLength;
		
		/// <summary>
		/// アクセサ構造体を用意する。
		/// </summary>
		public MotionDataAccessor CreateAccessor( int motionIndex )
		{
			var streamLength	= (int)KeyStreamSection.length * boneLength;
			var streamStart		= motionIndex * streamLength;

			return new MotionDataAccessor
			{
				streamSlices	= pool.StreamSlices.Slice( streamStart, streamLength ),
				boneParents		= pool.BoneParents.Slice(),
				boneLength		= boneLength,
				TimeLength		= pool.Motions[ motionIndex ].TimeLength
			};
		}


		public void Dispose()
		{
			if( pool.Motions.IsCreated )		pool.Motions.Dispose();
			if( pool.StreamSlices.IsCreated )	pool.StreamSlices.Dispose();
			if( pool.Keys.IsCreated )			pool.Keys.Dispose();
			if( pool.BoneParents.IsCreated )	pool.BoneParents.Dispose();
		}

	}


	/// <summary>
	/// モーションデータへのアクセサ構造体
	/// </summary>
	public struct MotionDataAccessor
	{
		internal NativeSlice<StreamSliceInNative>	streamSlices;

		internal NativeSlice<int>	boneParents;
		internal int				boneLength;
	

		public float	TimeLength		{ get; internal set; }

		public int GetParentBoneIndex( int boneIndex ) => boneParents[ boneIndex ];

		public StreamSliceInNative GetStreamSlice( int boneIndex, KeyStreamSection section )
		{
			if( streamSlices.Length == 0 ) return new StreamSliceInNative();
			if( boneIndex < 0 ) return new StreamSliceInNative();
			
			var sectionOffsetInMotion	= (int)section * boneLength;
			var streamOffsetInSection	= boneIndex;

			var addresss = sectionOffsetInMotion + streamOffsetInSection;

			return streamSlices[ addresss ];
		}
	}

	// ----------------------------------------------------------

	
	
	
	// モーションクリップからモーションデータへのコンバート ------------------

	public static partial class MotionClipUtility
	{
	
		static public void ConvertFrom( this MotionDataInNative dst, MotionClip src )
		{

			// ボーン
			var qParentBones =
				from parentPath in src.StreamPaths.Select( path => getParent(path) )	// ペアレントパス列挙
				join pathIndex in src.StreamPaths.Select( (path,i) => (path,i) )		// 順序数を保持する索引と外部結合する
					on parentPath equals pathIndex.path
					into pathIndexes
				from pathIndex in pathIndexes.DefaultIfEmpty( (path:"",i:-1) )			// 結合できないパスは -1
				select pathIndex.i
				;

			string getParent( string path ) => Path.GetDirectoryName(path).Replace("\\","/");


			// モーション
			var qMotions =
				from motion in src.MotionData.Motions
				select new MotionUnitInNative
				{
					TimeLength	= motion.TimeLength
				};



			// ソース全キー
			var qKeys =
				from motion in src.MotionData.Motions
				from section in motion.Sections
				from stream in section.Streams
				from key in Enumerable.Zip( stream.keys.Append(new KeyDataUnit()), stream.keys, (pre,now) => (pre,now) )
				select new KeyUnitInNative
				{
				//	Time	= new float4( key.now.Time - key.pre.Time, 0.0f, 0.0f, 0.0f ),
					Time	= new float4( key.now.Time, 0.0f, 0.0f, 0.0f ),
					Value	= key.now.Value
				};
		


			// インクリメンタルなキー位置を返す。苦肉の策。
			var iKeyInPool = 0;
			Func<StreamDataUnit, int>	indexResolver = stream =>
			{
				var i = iKeyInPool;
				iKeyInPool += stream.keys.Length;
				return i;
			};

			//var baseSectionList = new [] { "m_LocalPosition", "m_LocalRotation", "m_LocalScale" };

			// ストリームスライスの構築
			var qStreamSlices =
				
				// モーションごとの３セクション配列を生成する。
				from motion in src.MotionData.Motions
				let sections = motion.Sections.ToDictionary( section => section.SectionName )
				select new []
				{
					sections.GetOrDefault( "m_LocalPosition" ),
					sections.GetOrDefault( "m_LocalRotation" ),
					sections.GetOrDefault( "m_LocalScale" )
				}
				into sectionArray
			
				// セクションは空の場合がある。空セクションのストリームも確保し、すべてデフォルトスライスを配置させる。
				from section in sectionArray
				let streams = section.Streams?.ToDictionary( stream => stream.StreamPath )

				// ストリームも空の場合がある。空はデフォルトスライス。また並び順は src.StreamPaths の通り。 
				from streamPath in src.StreamPaths
				let stream = streams?.GetOrDefault( streamPath ) ?? new StreamDataUnit()
			
				select new StreamSliceInNative
				{
					Keys = stream.keys != null
						? new NativeSlice<KeyUnitInNative>( dst.pool.Keys, indexResolver(stream), stream.keys.Length )
						: new NativeSlice<KeyUnitInNative>()
				};
		
			

			// 実体を確保
			dst.pool.Motions		= qMotions.ToNativeArray( Allocator.Persistent );
			dst.pool.Keys			= qKeys.ToNativeArray( Allocator.Persistent );
			dst.pool.StreamSlices	= qStreamSlices.ToNativeArray( Allocator.Persistent );
			dst.pool.BoneParents	= qParentBones.ToNativeArray( Allocator.Persistent );
			dst.boneLength			= src.StreamPaths.Length;
		}

		
		static public void TransformKeyDataToWorld_( this MotionDataInNative motionData )
		{
			var qKeysPerMotions = 
				from ma in motionData.pool.Motions.Select( (x,i) => motionData.CreateAccessor(i) )
				from ibone in Enumerable.Range( 0, motionData.BoneLength )
				select (ma, ime:ibone, iparent:ma.GetParentBoneIndex(ibone))
				;

			foreach( var (ma, ime, iparent) in qKeysPerMotions )
			{
				if( iparent == -1 ) continue;

				Debug.Log( $"bone {ime} {iparent}" );
				var thisPositions	= ma.GetStreamSlice( ime, KeyStreamSection.positions ).Keys;
				var parentPositions	= ma.GetStreamSlice( iparent, KeyStreamSection.positions ).Keys;
				var thisRotations	= ma.GetStreamSlice( ime, KeyStreamSection.rotations ).Keys;
				var parentRotations	= ma.GetStreamSlice( iparent, KeyStreamSection.rotations ).Keys;
				
				var posNearKeys	= new StreamNearKeysCacheData();
				var posProgress	= new StreamTimeProgressData();
				var posShifter	= new StreamKeyShiftData(){ Keys = parentPositions };
				var rotNearKeys	= new StreamNearKeysCacheData();
				var rotProgress	= new StreamTimeProgressData();
				var rotShifter	= new StreamKeyShiftData(){ Keys = parentRotations };
				
				rotNearKeys.InitializeKeys( ref rotShifter, ref rotProgress );
				
				for( var i = 0; i < thisRotations.Length ; i++ )
				{
					var thisRot = thisRotations[i];
					
					rotProgress.TimeProgress = thisRot.Time.x;
					do rotNearKeys.ShiftKeysIfOverKeyTime( ref rotShifter, in rotProgress );
					while( rotProgress.TimeProgress > rotNearKeys.Time_To );
					
					var lrot	= new quaternion( thisRot.Value );
					var prot	= new quaternion( rotNearKeys.Interpolate(rotNearKeys.CaluclateTimeNormalized(rotProgress.TimeProgress)) );

					thisRot.Value = math.mul( prot, lrot ).value;
					thisRotations[i] = thisRot;
				//	Debug.Log($"{thisRot.Time} {thisRot.Value}");
				}
				
				posNearKeys.InitializeKeys( ref posShifter, ref posProgress );
				rotNearKeys.InitializeKeys( ref rotShifter, ref rotProgress );

				for( var i = 0; i < thisPositions.Length; i++ )
				{
					var thisPos = thisPositions[i];

					posProgress.TimeProgress = thisPos.Time.x;
					do StreamUtility.ShiftKeysIfOverKeyTime( ref posNearKeys, ref posShifter, in posProgress );
					while( posProgress.TimeProgress > posNearKeys.Time_To );

					rotProgress.TimeProgress = thisPos.Time.x;
					do StreamUtility.ShiftKeysIfOverKeyTime( ref rotNearKeys, ref rotShifter, in rotProgress );
					while( rotProgress.TimeProgress > rotNearKeys.Time_To );

					var lpos    = thisPos.Value.xyz;
					var ppos    = posNearKeys.Interpolate(posNearKeys.CaluclateTimeNormalized(posProgress.TimeProgress));
					var prot    = new quaternion( rotNearKeys.Interpolate(rotNearKeys.CaluclateTimeNormalized(rotProgress.TimeProgress)) );

					thisPos.Value = math.mul( prot, lpos ).As_float4() + ppos;
					thisPositions[i] = thisPos;
					//Debug.Log( $"{thisPos.Time} {thisPos.Value}" );
				}
			}
			
		}
		static public void TransformKeyDataToWorld( this MotionDataInNative motionData )
		{
			var rotationKeyLength = motionData.StreamPool
				.Buffer( 3 )
				.Select( x => x[(int)KeyStreamSection.rotations].Keys.Length )
				.Sum()
				;
			var dst = new NativeArray<KeyUnitInNative>( rotationKeyLength * 2, Allocator.Persistent );
			
			var qKeysPerMotions = 
				from ma in motionData.pool.Motions.Select( (x,i) => motionData.CreateAccessor(i) )
				from ibone in Enumerable.Range( 0, motionData.BoneLength )
				select (ma, ime:ibone, iparent:ma.GetParentBoneIndex(ibone))
				;

			foreach( var (ma, ime, iparent) in qKeysPerMotions )
			{
				//if( iparent == -1 ) continue;
				if( iparent == -1 )
				{
					
				}

				Debug.Log( $"bone {ime} {iparent}" );
				var thisPositions	= ma.GetStreamSlice( ime, KeyStreamSection.positions ).Keys;
				var parentPositions	= ma.GetStreamSlice( iparent, KeyStreamSection.positions ).Keys;
				var thisRotations	= ma.GetStreamSlice( ime, KeyStreamSection.rotations ).Keys;
				var parentRotations	= ma.GetStreamSlice( iparent, KeyStreamSection.rotations ).Keys;
				
				var posNearKeys	= new StreamNearKeysCacheData();
				var posProgress	= new StreamTimeProgressData();
				var posShifter	= new StreamKeyShiftData(){ Keys = parentPositions };
				var rotNearKeys	= new StreamNearKeysCacheData();
				var rotProgress	= new StreamTimeProgressData();
				var rotShifter	= new StreamKeyShiftData(){ Keys = parentRotations };
				
				rotNearKeys.InitializeKeys( ref rotShifter, ref rotProgress );

				for( var i = 0; i < thisRotations.Length ; i++ )
				{
					var thisRot = thisRotations[i];
					
					rotProgress.TimeProgress = thisRot.Time.x;
					do StreamUtility.ShiftKeysIfOverKeyTime( ref rotNearKeys, ref rotShifter, in rotProgress );
					while( rotProgress.TimeProgress > rotNearKeys.Time_To );
					
					var lrot	= new quaternion( thisRot.Value );
					var prot	= new quaternion( rotNearKeys.Interpolate(rotNearKeys.CaluclateTimeNormalized(rotProgress.TimeProgress)) );

					thisRot.Value = math.mul( prot, lrot ).value;
					thisRotations[i] = thisRot;
				//	Debug.Log($"{thisRot.Time} {thisRot.Value}");
				}
				
			}
			
		}

	}

	// ----------------------------------------------------------

}