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
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;

using Abss.Geometry;
using Abss.Cs;
using Abss.Misc;
using Abss.Motion;
using Abss.Obj.Entities;
using Abss.Model;
using Abss.Draw.Cs;

namespace Abss.Draw.Cs
{


	[UpdateAfter( typeof(MotionProgressSystem) )]
	public class DrawMeshCsSystem : ComponentSystem
	{
		
		//[Inject]
		CreateModelSystem	modelCreateSystem;
		
		
		SimpleIndirectArgsManualyBuffer			argsBuffer;
		SimpleComputeBuffer<BonePostureData>	bonesBuffer;


		readonly int	bindPoses_ShaderId	= Shader.PropertyToID("bindPoses");
		readonly int	boneLength_ShaderId	= Shader.PropertyToID("boneLength");
		readonly int	positions_ShaderId	= Shader.PropertyToID("positions");
		readonly int	rotations_ShaderId	= Shader.PropertyToID("rotations");

		const int total_bone_buffer_length	= 10000;

		public DrawMeshCsSystem()
		{
			//this.bonesBuffer	= new SimpleComputeBuffer<BonePostureData>( "bones", total_bone_buffer_length );
			//this.argsBuffer		= new SimpleIndirectArgsManualyBuffer( this.models );
			
		}


		protected override void OnCreateManager()
		{
			base.OnCreateManager();
			
		}

		protected override void OnDestroyManager()
		{
			base.OnDestroyManager();
		}

		protected override unsafe void OnUpdate()
		{

		}
		
		

	}
	


	public struct ModelCsDrawer
	{
		public readonly SimpleComputeBuffer<BonePostureData>	models;
		public readonly SimpleComputeBuffer<BonePostureData>	bones;
		public readonly SimpleIndirectArgsManualyBuffer			args;

		public void SetOrder( int index, Mesh mesh, Material mat )
		{

		}
	}
	//public struct ModelDrawUnit
	//{
	//	public readonly int	boneLength;
	//	public readonly int	
	//}
}