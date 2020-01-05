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


using Abss.Arthuring;
using Abss.Motion;
using Abss.SystemGroup;
using Abss.Misc;

namespace Abss.Draw
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


        protected override void OnDestroy()// 暫定（もっとわかりやすい場所で
        {
            var cb = this.GetSingleton<DrawSystemComputeTransformBufferData>();
            cb.Transforms.Dispose();

            var eq = this.EntityManager.CreateEntityQuery( typeof(DrawModelComputeArgumentsBufferData) );
            using( eq )
            {
                var args = eq.ToComponentDataArray<DrawModelComputeArgumentsBufferData>();
                foreach(var arg in args)
                {
                    arg.InstanceArgumentsBuffer.Dispose();
                }
            }
        }
    }
}
