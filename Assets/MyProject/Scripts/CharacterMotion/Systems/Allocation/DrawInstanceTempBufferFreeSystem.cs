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

using Abss.Cs;
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
    [UpdateInGroup( typeof( DrawAllocationGroup ) )]
    //[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    //[UpdateInGroup( typeof( DrawSystemGroup ) )]
    public class DrawInstanceTempBufferFreeSystem : ComponentSystem
    {


        protected override void OnStartRunning()
        {
            this.drawMeshCsSystem = this.World.GetExistingSystem<DrawMeshCsSystem>();
        }



        protected override void OnUpdate()
        {


            this.tempBufferSystem.TempInstanceBoneVectors.Dispose();
            // 本体と離れているので忘れないよう…


        }

    }
}
