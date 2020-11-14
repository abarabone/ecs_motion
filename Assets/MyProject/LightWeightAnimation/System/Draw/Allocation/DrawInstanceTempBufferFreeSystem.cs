using Unity.Entities;

namespace Abarabone.Draw
{
    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    [UpdateAfter( typeof( DrawMeshCsSystem ) )]
    public class DrawInstanceTempBufferFreeSystem : ComponentSystem
    {




        protected override void OnUpdate()
        {

            this.Entities
                .ForEach(
                    ( ref DrawSystem.NativeTransformBufferData buf ) =>
                    {
                        buf.Transforms.Dispose();
                    }
                );

        }

    }
}
