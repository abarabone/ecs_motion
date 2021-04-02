﻿using Unity.Entities;

namespace Abarabone.Draw
{
    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawSystemGroup ) )]
    [UpdateAfter( typeof( DrawMeshCsSystem ) )]
    public class DrawInstanceTempBufferFreeSystem : DependsDrawCsSystemBase
    {

        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<DrawSystem.TransformBufferUseTempJobTag>();
        }


        protected override void OnUpdateWith()
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
