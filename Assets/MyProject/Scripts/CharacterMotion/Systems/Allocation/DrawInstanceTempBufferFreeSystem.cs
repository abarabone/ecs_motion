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
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawAllocationGroup ) )]
    //[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    //[UpdateInGroup( typeof( DrawSystemGroup ) )]
    public class DrawInstanceTempBufferFreeSystem : ComponentSystem
    {


        protected override void OnStartRunning()
        {

        }



        protected override void OnUpdate()
        {




        }

    }
}
