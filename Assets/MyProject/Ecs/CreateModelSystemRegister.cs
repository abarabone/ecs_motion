using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System.Linq;
using UniRx;

using Abss.Motion;
using Abss.Draw;
using Abss.Geometry;

namespace Abss.Model
{
	/// <summary>
	/// システムの登録をコンポーネントの enable/disable によって制御する。
	/// </summary>
	public sealed class CreateModelSystemRegister
		: Ecs.EcsStarter._SystemRegistMonoBehaviourWithParameter<CreateModelSystem>
	{
		/// <summary>
		/// メッシュやマテリアル等のモデルデータセットする。
		/// </summary>
		public ModelAssetData[]	ModelDatas;
	}

	[System.Serializable]
	public class ModelAssetData
	{
		public Mesh			mesh;
		public Material		material;
		public MotionClip	motionClip;
	}

}