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
using Abss.Obj.Entities;


namespace Abss.Motion
{
	

	public class MotionBootStrap : MonoBehaviour
	{
		
		public GameObject[]		ObjectsWithAnimationClip;

		public MotionClip		MotionClipData;

		
		MotionDataInNative	motionData;

		struct MeshInstanceRenderer
		{
			public Mesh	mesh;
			public Material material;
		}

		[SerializeField]
		MeshInstanceRenderer	meshSetting;


		//public int motionIndex;
		public IntReactiveProperty	motionIndex = new IntReactiveProperty( 0 );


		public int num;

		private void OnDestroy()
		{
			if( motionData != null ) motionData.Dispose();
		}


		//private async void Awake()
		private void Start()
		{
			

			var md	= new MotionDataInNative();
			md.ConvertFrom( MotionClipData );
		//	md.TransformKeyDataToWorld_();
			
			this.motionData	= md;



			// メッシュの加工、差し替え

			var newmesh = new Mesh();
			newmesh.vertices	= meshSetting.mesh.vertices;
			newmesh.boneWeights	= meshSetting.mesh.boneWeights;
			newmesh.bindposes	= meshSetting.mesh.bindposes;
			newmesh.normals		= meshSetting.mesh.normals;
			newmesh.uv			= meshSetting.mesh.uv;
			newmesh.triangles	= meshSetting.mesh.triangles;
			meshSetting.mesh	= newmesh.AddBoneInfoFrom( useUvChannel:1, this.MotionClipData );
			meshSetting.mesh.RecalculateBounds();//.bounds		= meshSetting.mesh.bounds;
			meshSetting.mesh.UploadMeshData( markNoLongerReadable:true );


			// マテリアルの差し替え

			var newmat = new Material( meshSetting.material );

			var bindposes = meshSetting.mesh.bindposes;
			newmat.SetMatrixArray( "mts", bindposes );
			newmat.SetInt( "boneLength", bindposes.Length );

			newmat.SetVectorArray( "positions", new Vector4 [1023] );
			newmat.SetVectorArray( "rotations", new Vector4 [1023] );
			
			newmat.enableInstancing = true;

			meshSetting.material	= newmat;


			
			var em	= World.Active.GetOrCreateManager<EntityManager>();

			// レンダラ
			
			//var rendererArch= em.CreateArchetype(
			//	typeof(MeshInstanceRenderer)
			//);

			//var rent = em.CreateEntity( rendererArch );
			//em.SetSharedComponentData( rent, meshSetting );


			// アーキタイプ

			var chArch		= em.CreateArchetype(
				typeof(ObjectPostureData),
				typeof(ObjectInitializeLabel)
			);

			Enumerable
				.Range(0, num)
				.Select(x => (i: x, ent: em.CreateEntity(chArch)))
				.Do(x => em.SetComponentData(x.ent, new ObjectInitializeLabel { ObjectEntity = x.ent }))
				.ForEach(x => em.SetComponentData(x.ent,
				   new ObjectPostureData
				   {
					   position = new float3(x.i % 10, (x.i / 10) % 10, -x.i / 100.0f),
					   rotation = quaternion.identity,
					   scale = new float3(1, 1, 1),
				   }
			   ));
			return;
			var streamArch  = em.CreateArchetype(
				typeof(StreamInitialLabel),
				typeof(StreamKeyShiftData),
				typeof(StreamNearKeysCacheData),
				typeof(StreamInterpolatedData)
			);
			var boneArch    = em.CreateArchetype(
				typeof(BoneDrawingInFrameLabel),
				typeof(BoneEntityLinkData),
			//	typeof(BoneIndexData),
				typeof(BonePostureData)
			);
			var motionArch	= em.CreateArchetype(
				typeof(ObjectDrawingInFrameLabel),
				typeof(MotionInitializeData),
				typeof(MotionInfoData)
			);


			var blendArch	= em.CreateArchetype(
				typeof(BlendBoneSourceLinkData),
				typeof(BlendBoneWeightData)
			);
			

			var drawOrderId	= 0;
			var boneSerialIndex = 0;


			//Enumerable
			//	.Range(0, num)
			//	.Select(x => (ent: create(UnityEngine.Random.Range(0, 15)), i: drawOrderId - 1))
			//	.ForEach(x =>
			//	   x.ent.AddComponentData(em,
			//		   new ObjectPostureData
			//		   {
			//			   position = new float3(x.i % 10, (x.i / 10) % 10, -x.i / 100.0f),
			//			   rotation = quaternion.identity,
			//			   scale = new float3(1, 1, 1),
			//		   }
			//	   )
			//	);

			//	Observable.EveryGameObjectUpdate().Take(num).Subscribe( x => create( Random.Range( 0, 15 ) ) ).AddTo( gameObject );
			//	motionIndex.Subscribe( x => create(x) ).AddTo( gameObject );



			//createBlendBone( 3, 10 );

			//void createBlendBone( int motionIndex0, int motionIndex1 )
			//{
			//	var ma = md.CreateAccessor( 0 );

			//	using( var boneEntities	= new NativeArray<Entity>( 16, Allocator.Temp ) )
			//	{
			//		em.CreateEntity( boneArch, boneEntities );

			//		boneEntities.ForEach( (ent, ibone) =>
			//		{

			//			em.SetComponentData( ent,
			//				new BoneDrawTargetLabel
			//				{
			//					drawingOrder = drawOrderId,
			//					boneIndex = ibone,
			//				}
			//			);
			//		});
			//	}
			//}



			#if false
			Entity create( int motionIndex )
			{
				
				var ma = md.CreateAccessor( motionIndex );



				
				// ボーン＆ストリーム

				var nowTime	= Time.time;
				Entity boneTop;

				using( var boneEntities	= new NativeArray<Entity>( ma.boneLength, Allocator.Temp ) )
				using( var streamEntities = new NativeArray<Entity>( ma.boneLength * 2, Allocator.Temp ) )
				{
					em.CreateEntity( boneArch, boneEntities );
					em.CreateEntity( streamArch, streamEntities );

					boneTop = boneEntities[ 0 ];

					Enumerable.Zip( boneEntities, streamEntities.Buffer(2), (bone, streams) => (bone, streams) )
					.ForEach( (ents, ibone) =>
					{

						var parentIndex = ma.GetParentBoneIndex( ibone );
						var siblingIndex = ibone + 1;

						em.SetComponentData( ents.bone,
							new BoneEntityLinkData
							{
							//	Self	= ents.bone,
								NextEntity	= siblingIndex < ma.boneLength ? boneEntities[siblingIndex] : Entity.Null,
								ParentEntity	= parentIndex > -1 ? boneEntities[parentIndex] : Entity.Null,
								positionEntity	= ents.streams[0],
								rotationEntity	= ents.streams[1],
							}
						);
						//em.SetComponentData( ents.bone,
						//	new BoneIndexData
						//	{
						//		Index		= ibone,
						//		ParentIndex	= parentIndex,
						//	}
						//);
						em.SetComponentData( ents.bone,
							new BoneDrawingInFrameLabel
							{
								boneSerialIndex = drawOrderId * ma.boneLength + ibone,
							}
						);

						//	em.AddComponentData( ents.streams[0], new StreamInitialData() );
						//	if( ibone != 0 ) em.AddComponentData( ents.streams[0], new StreamInitialDataFor1pos() );

						createStreamSectionEntity( KeyStreamSection.positions, 0.1f );
						createStreamSectionEntity( KeyStreamSection.rotations, 0.1f );

						void createStreamSectionEntity( KeyStreamSection streamSection, float timeScale )
						{
						//	em.AddComponentData( ents.streams[(int)streamSection], new StreamInitialData() );
							em.SetComponentData( ents.streams[(int)streamSection],
								new StreamKeyShiftData
								{
								//	Self	= ents.streams[(int)streamSection],
									Keys	= ma.GetStreamSlice( ibone, streamSection ).Stream
								}
							);
							em.SetComponentData( ents.streams[(int)streamSection],
								new StreamNearKeysCacheData
								{
									TimeLength	= ma.TimeLength,
									TimeScale	= 0.1f
								}
							);
						}

					} );

				}
				
				
				// モーション

				var motionEntity	= em.CreateEntity( motionArch );

			//	em.AddComponent( motionEntity, typeof(MotionInitializeData) );
				em.SetComponentData<ObjectDrawingInFrameLabel>( motionEntity, new ObjectDrawingInFrameLabel { orderId = drawOrderId++ } );

				em.SetComponentData<MotionInfoData>( motionEntity,
					new MotionInfoData
					{
						MotionIndex		= motionIndex,
						DataAccessor	= ma,
					//	Self			= motionEntity,
						//BoneEntityTop			= boneTop
					}
				);
				em.SetComponentData<MotionInitializeData>( motionEntity,
					new MotionInitializeData
					{
					}
				);


				return motionEntity;
			}
			#endif
			//await this.OnDestroyAsObservable();
		}
		
		

		public void CreateCharacterEntity( EntityManager em, float3 pos, quaternion rot, float3 scale )
		{

		}

		public void CreateBlendBoneEntities(  EntityManager em )
		{

		}


	}


}