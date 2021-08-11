using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Draw
{

    using DotsLite.Misc;
    using DotsLite.SystemGroup;
    using DotsLite.Utilities;
    using DotsLite.Dependency;

    /// <summary>
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Sort))]
    public class SortModelBufferSystem : SystemBase
    {



        protected override unsafe void OnUpdate()
        {
            this.Entities
                //.WithBurst()
                .ForEach(
                    (
                        in DrawModel.InstanceCounterData counter,
                        in DrawModel.InstanceOffsetData offset
                    ) =>
                    {
                        if (counter.InstanceCounter.Count == 0) return;

                        var p = offset.pVectorOffsetPerModelInBuffer;
                        var length = counter.InstanceCounter.Count;

                        //UnsafeUtility.MemCpyStride(pDst, dstspan, pSrc, srcspan, elementSize, count);


                        //NativeSortExtension.Sort(p, length), ;



                    }
                )
                ;
        }

    }
}
