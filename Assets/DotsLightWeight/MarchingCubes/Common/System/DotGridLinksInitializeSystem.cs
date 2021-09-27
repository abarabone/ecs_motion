//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.MarchingCubes
//{
//    using MarchingCubes;
//    using DotsLite.Draw;

//    //[DisableAutoCreation]
//    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
//    [UpdateInGroup(typeof(InitializationSystemGroup))]
//    public class DotGridLinksInitializeSystem : SystemBase
//    {

//        protected override unsafe void OnUpdate()
//        {

//            this.Entities
//                .WithName("GridArea")
//                .WithoutBurst()
//                .WithAll<DotGridArea.InitializeData>()
//                .ForEach(
//                    (
//                        Entity ent,
//                        ref DotGridArea.LinkToGridData links,
//                        in DotGridArea.InfoData info
//                    ) =>
//                    {

//                        var length = info.GridLength.x * info.GridLength.y * info.GridLength.z;
//                        links.Grids = new UnsafeList<DotGrid32x32x32Unsafe>(length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
//                        links.Grids.length = length;

//                        // とりあえず
//                        links.Grids[length >> 1] = DotGrid32x32x32Unsafe.CreateDefaultCube(GridFillMode.Solid);

//                    }
//                )
//                .Run();
//        }

//    }
//}
