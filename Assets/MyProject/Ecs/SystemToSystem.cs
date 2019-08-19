using Unity.Collections;
using Unity.Entities;

using Abss.Motion;
using Abss.Draw;
using Abss.Draw.Cs;
using Abss.Misc;

namespace Abss.Ecs
{

	public class SystemToSystem : ComponentSystem
	{

		internal ThreadSafeAdditiveArrayTempJob<Entity>	cullingToMotion;
		


		protected override void OnUpdate()
		{}
	}

	public interface ISystemToSystemAsTempJob<TSrcSystem, TDstSystem>
	{

	}

}


