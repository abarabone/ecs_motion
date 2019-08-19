using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;

using Abss.Geometry;
using Abss.Misc;
using Abss.Draw.Cs;
using Abss.Draw;
using us = Unity.Collections.LowLevel.Unsafe.UnsafeUtility;
using usx = Unity.Collections.LowLevel.Unsafe.UnsafeUtilityEx;
using Abss.Motion;


namespace Abss.Model
{

	/// <summary>
	/// モデルエンティティの生成システム
	/// </summary>
	public sealed class CreateModelSystem : JobComponentSystem
	{

		public readonly ModelInfoForEntity[]	Models;
		// mesh/mat/motion が null のモデルは除外してある。
		
		public readonly ModelEntityCreator		EntityCreator;
		// アーキタイプ等を所持する。
		
		
		public CreateModelSystem( CreateModelSystemRegister register )
		{

			this.Models = createModelInfoArray( register.ModelDatas );
			
			this.EntityCreator = new ModelEntityCreator( register.MainWorld );


			// とりあえず一体分作成
			this.EntityCreator.CreateMotionEntities
				( register.MainWorld, Models[ 0 ], motionIndex: UnityEngine.Random.Range( 0, 10 ) );
			Models[ 0 ].ModelInstanceCount++;

			return;


			ModelInfoForEntity[] createModelInfoArray( ModelAssetData[] modelDatas )
			{
				return modelDatas
					.Where( model => model != null )
					.Where( model => model.mesh != null && model.material != null && model.motionClip != null )
					.Select( model => new ModelInfoForEntity(model) )
					.ToArray();
			}
		}


		protected override void OnCreateManager()
		{
			base.OnCreateManager();
		}

		protected override void OnDestroyManager()
		{
			foreach( var m in this.Models ) m.Dispose();
			
			base.OnDestroyManager();
		}

		protected override JobHandle OnUpdate( JobHandle inputDeps )
		{
			return inputDeps;//base.OnUpdate( inputDeps );
		}
	}
	

	/// <summary>
	/// 
	/// </summary>
	public struct ModelEntityCreator
	{

		EntityArchetype		motionArchetype;

		
		/// <summary>
		/// モデル１体分の各種エンティティを生成する。
		/// </summary>
		public void CreateMotionEntities( World world, ModelInfoForEntity modelData, int motionIndex )
		{

			var em	= world.GetOrCreateSystem<EntityManager>();

			var ent = em.CreateEntity( this.motionArchetype );
			
			var ma	= modelData.CreateMotionAccessor( motionIndex );


			em.SetComponentData<MotionInfoData>
			(
				ent,
				new MotionInfoData
				{ 
					MotionIndex		= 0,
					DataAccessor	= ma
				}
			);
			em.SetComponentData<DrawTargetSphere>
			(
				ent,
				new DrawTargetSphere
				{
					center	= 0,
					radius	= 1
				}
			);
			em.SetComponentData<DrawModelInfo>
			(
				ent,
				new DrawModelInfo
				{
					modelIndex	= 0
				}
			);
			var buf = em.GetBuffer<MotionStreamElement>( ent );
			for( var i = 0; i < ma.boneLength * 2 ; ++i )
			{
				var stream = new MotionStreamElement();

				stream.Progress.TimeLength	= ma.TimeLength;
				stream.Progress.TimeScale	= 1.0f;
				stream.Shift.Keys = ma.GetStreamSlice( i >> 2, KeyStreamSection.positions + (i & 1) ).Keys;

				StreamUtility.InitializeKeys( ref stream.Cache, ref stream.Shift, ref stream.Progress );

				buf.Add( stream );
			}
		}
		

		/// <summary>
		/// 初期化
		/// </summary>
		public ModelEntityCreator( World world )
		{

			this.motionArchetype = createArchetype();
			
			return;
			

			EntityArchetype createArchetype()
			{
				var em	= world.GetOrCreateSystem<EntityManager>();

				return em.CreateArchetype
				(
					ComponentType.Create<MotionInfoData>(),
					ComponentType.Create<MotionStreamElement>(),
					ComponentType.Create<DrawTargetSphere>(),
					ComponentType.Create<DrawModelInfo>()
				);
			}
		}

	}

	
	/// <summary>
	/// モデル１体のエンティティ生成に必要なデータの集合
	/// </summary>
	public class ModelInfoForEntity : System.IDisposable
	{
		
		readonly MotionDataInNative	md;

		public readonly Mesh		MeshConverted;
		public readonly Material	Material;
		
		public int ModelInstanceCount;
		// 現在生成されているモデル数、このクラスはデータということなので、いずれは配置を見直したい。

		public int BoneLength => md.BoneLength;

		/// <summary>
		/// モーションへのアクセス構造体を生成して返す。
		/// </summary>
		public MotionDataAccessor CreateMotionAccessor( int motionIndex ) => md.CreateAccessor( motionIndex );
		

		/// <summary>
		/// 初期化
		/// </summary>
		public ModelInfoForEntity( ModelAssetData modelData )
		{
			//if( modelData.mesh == null ) return;
			//if( modelData.material == null ) return;
			//if( modelData.motionClip == null ) return;

			this.md				= convertMotionData();
			this.MeshConverted	= convertMesh();
			this.Material		= modelData.material;

			return;


			MotionDataInNative convertMotionData()
			{
				var md = new MotionDataInNative();
				md.ConvertFrom( modelData.motionClip );
				return md;
			}

			Mesh convertMesh()
			{
				var newmesh = new Mesh();

				newmesh.vertices	= MeshUtility.ConvertVertices( modelData.mesh.vertices, modelData.mesh.boneWeights, modelData.mesh.bindposes );
				newmesh.triangles	= modelData.mesh.triangles;
				newmesh.normals		= modelData.mesh.normals;
				newmesh.uv			= modelData.mesh.uv;
				newmesh.colors32	= MeshUtility.CreateBoneIndexColors( modelData.mesh.boneWeights, modelData.motionClip );
				var wuvs			= MeshUtility.CreateBoneWeightUvs( modelData.mesh.boneWeights, modelData.motionClip );
				newmesh.SetUVs( channel:1, wuvs );
				newmesh.RecalculateBounds();
				newmesh.UploadMeshData( markNoLongerReadable:true );

				return newmesh;
			}
		}
		
		/// <summary>
		/// 内部リソースの破棄
		/// </summary>
		public void Dispose()
		{
			if( md != null ) md.Dispose();
			Object.Destroy( MeshConverted );
		}

	}
}
