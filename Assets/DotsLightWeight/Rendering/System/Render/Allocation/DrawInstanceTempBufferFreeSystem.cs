using Unity.Entities;

namespace DotsLite.Draw
{
    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Render.DrawAfter.TempFree ) )]
    //[UpdateAfter( typeof( DrawMeshCsSystem ) )]
    public partial class DrawInstanceTempBufferFreeSystem : SystemBase
    {

        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<DrawSystem.TransformBufferUseTempJobTag>();
        }


        protected override void OnUpdate()
        {

            this.Entities
                .ForEach(
                    ( ref DrawSystem.NativeTransformBufferData buf ) =>
                    {
                        buf.Transforms.Dispose();
                    }
                )
                .Run();

        }

    }
}
