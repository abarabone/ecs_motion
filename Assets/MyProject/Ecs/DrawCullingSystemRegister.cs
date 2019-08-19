
namespace Abss.Draw
{
	/// <summary>
	/// システムの登録をコンポーネントの enable/disable によって制御する。
	/// </summary>
	public sealed class DrawCullingSystemRegister
		: Ecs.EcsStarter._SystemRegistMonoBehaviour<DrawCullingSystem>
	{}
}