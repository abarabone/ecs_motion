using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Physics;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotsLite.MarchingCubes
{
    using DotsLite.Dependency;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class DotGridToCollisionSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        public DotGridUpdateSystem MessageHolderSystem;




        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DotGridCopyToGpuSystem>(this);

            this.MessageHolderSystem = this.World.GetOrCreateSystem<DotGridUpdateSystem>();
        }

        protected override void OnUpdate()
        {
            using var barScope = this.bardep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                KeyEntities = this.MessageHolderSystem.Reciever.Holder.keyEntities.AsDeferredJobArray(),
                mcdata = this.GetSingleton<MarchingCubeGlobalData>().Assset,
                grids = this.GetComponentDataFromEntity<DotGrid.UnitData>(isReadOnly: true),
                parents = this.GetComponentDataFromEntity<DotGrid.ParentAreaData>(isReadOnly: true),
                poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true),
                areas = this.GetComponentDataFromEntity<DotGridArea.LinkToGridData>(isReadOnly: true),
            }
            .Schedule(this.MessageHolderSystem.Reciever.Holder.keyEntities, 32, this.Dependency);
        }



        [BurstCompile]
        unsafe struct JobExecution : IJobParallelForDefer
        {
            [ReadOnly]
            public NativeArray<Entity> KeyEntities;
            [ReadOnly]
            public BlobAssetReference<MarchingCubesBlobAsset> mcdata;

            [ReadOnly]
            public ComponentDataFromEntity<DotGrid.UnitData> grids;
            [ReadOnly]
            public ComponentDataFromEntity<DotGrid.ParentAreaData> parents;

            [ReadOnly]
            public ComponentDataFromEntity<DotGridArea.LinkToGridData> areas;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> poss;


            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute(int index)
            {
                var ent = this.KeyEntities[index];
                var grid = this.grids[ent];
                var parent = this.parents[ent];
                var pos = this.poss[ent];
                var area = this.areas[parent.ParentArea];

                makeMesh_(in grid, in pos, in area, in mcdata);


            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void makeMesh_(
        //static unsafe BlobAssetReference<Collider> makeMesh_(
            in DotGrid.UnitData grid, in Translation pos, in DotGridArea.LinkToGridData grids, in BlobAssetReference<MarchingCubesBlobAsset> mcdata)
        {
            var near = grids.PickupNearGridIds(grid);
            var writer = new MakeCube.MeshWriter
            {
                gridpos = pos.Value,
                vtxs = new NativeList<float3>(Allocator.Temp),
                tris = new NativeList<int3>(Allocator.Temp),
                //filter = 
                mcdata = mcdata,
            };

            MakeCube.SampleAllCubes(in near, ref writer);
            //var mesh = writer.CreateMesh();

            writer.Dispose();
            return;// mesh;
        }

        static unsafe void makeCubes_(uint *p)
        {
            for (var i = 0; i < 32 * 32; i++)
            {
                var xline = p[i];
                for (var ix = 0; ix < 32; ix++)
                {
                    var iz = i & 0x1f;
                    var iy = i >> 5 & 0x1f;



                    xline >>= 1;
                }
            }
            //for (var iy = 0; iy < 32; iy++)
            //{
            //    for (var iz = 0; iz < 32; iz++)
            //    {
            //        var xline = p[];
            //        for (var ix = 0; ix < 32; ix++)
            //        {
            //        }
            //    }
            //}
            //for (var i = 0; i < 32 * 32 * 32; i++)
            //{
            //    //var i3 = i.xxx() >> new int3(0, 10, 5) & 0x1f.xxx();
            //    var ix = i & 0x1f;
            //    var iz = i >> 5 & 0x1f;
            //    var iy = i >> 10 & 0x1f;
            //    p[i >> 5];
            //}
        }
    }

    static class iex
    {
        public static int3 xxx(this int i) => new int3(i, i, i);
    }
}
