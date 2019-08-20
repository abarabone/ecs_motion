
namespace Abss.Draw.Cs
{
	/// <summary>
	/// システムの登録をコンポーネントの enable/disable によって制御する。
	/// </summary>
	public class DrawCsWithMotionSystemRegister
		: Ecs.EcsStarter._SystemRegistMonoBehaviourWithParameter<DrawCsWithMotionSystem>
	{
		public Unity.Rendering.MeshInstanceRenderer[]	MeshInfos;
	}
}