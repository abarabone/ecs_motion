using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;


using Abarabone.Arthuring;
using Abarabone.Motion;
using Abarabone.SystemGroup;
using Abarabone.Misc;

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
                    ( ref DrawSystemNativeTransformBufferData buf ) =>
                    {
                        buf.Transforms.Dispose();
                    }
                );

        }

    }
}
