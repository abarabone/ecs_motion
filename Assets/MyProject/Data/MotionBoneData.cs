using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using UniRx.Triggers;
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


	//public struct BoneMotionLabel : IComponentData
	//{}

	//public struct BoneDrawableLabel : IComponentData
	//{}

	public struct BoneEntityLinkData : IComponentData
	{
        public Entity NextEntity;
        public Entity ParentBoneEntity;

        public Entity	PositionStreamEntity;
		public Entity	RotationStreamEntity;
	}
	
	//public struct BoneIndexData : IComponentData	//初期化時だけあればいいか？
	//{
	//	public int  Index;
	//	public int  ParentIndex;
	//}

	public struct BonePostureData : IComponentData
	{
		public float4		position;
		public quaternion	rotation;
	//	public float3		scale;
	}

	public struct BoneDrawingInFrameLabel : IComponentData
	{
		public int	boneSerialIndex;	// drawModelIndex * boneLength + boneIndex
	}


    public struct BoneStreamLinkData : IComponentData
    {
        public Entity PositionStreamEntity;
        public Entity RotationStreamEntity;
    }

    public struct BoneLevel0Data : IComponentData
    {
        
    }
    public struct BoneLevel1Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel2Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel3Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel4Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel5Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel6Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel7Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel8Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }
    public struct BoneLevel9Data : IComponentData
    {
        public Entity ParentBoneEntity;
    }


}