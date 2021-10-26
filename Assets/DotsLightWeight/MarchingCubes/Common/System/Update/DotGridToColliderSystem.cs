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
    using DotsLite.Collision;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]// çƒçlÇÃïKóvÇ†ÇË
    [UpdateAfter(typeof(DotGridUpdateSystem))]
    public class DotGridToCollisionSystem<TGrid> : DependencyAccessableSystemBase
        where TGrid : struct, IDotGrid<TGrid>
    {

        CommandBufferDependency.Sender cmddep;

        BarrierDependency.Sender bardep;

        public DotGridUpdateSystem MessageHolderSystem;




        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.bardep = BarrierDependency.Sender.Create<DotGridCopyToGpuSystem<TGrid>>(this);

            this.MessageHolderSystem = this.World.GetOrCreateSystem<DotGridUpdateSystem>();
        }

        protected unsafe override void OnUpdate()
        {
            using var cmdscope = this.cmddep.WithDependencyScope();
            using var barScope = this.bardep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                cmd = cmdscope.CommandBuffer.AsParallelWriter(),
                KeyEntities = this.MessageHolderSystem.Reciever.Holder.keyEntities.AsDeferredJobArray(),
                mcdata = this.GetSingleton<Global.MainData<TGrid>>().Assset,
                //pDefualtBlankGrid = this.GetSingleton<Global.MainData<TGrid>>().DefaultGrids[(int)GridFillMode.Blank].pXline,
                grids = this.GetComponentDataFromEntity<DotGrid<TGrid>.UnitData>(isReadOnly: true),
                parents = this.GetComponentDataFromEntity<DotGrid<TGrid>.ParentAreaData>(isReadOnly: true),
                poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true),
                areas = this.GetComponentDataFromEntity<DotGridArea.LinkToGridData>(isReadOnly: true),
                colliders = this.GetComponentDataFromEntity<PhysicsCollider>(isReadOnly: true),
            }
            .Schedule(this.MessageHolderSystem.Reciever.Holder.keyEntities, 1, this.Dependency);
        }



        [BurstCompile]
        unsafe struct JobExecution : IJobParallelForDefer
        {
            public EntityCommandBuffer.ParallelWriter cmd;

            [ReadOnly]
            public NativeArray<Entity> KeyEntities;
            [ReadOnly]
            public BlobAssetReference<MarchingCubesBlobAsset> mcdata;

            [ReadOnly]
            public ComponentDataFromEntity<DotGrid<TGrid>.UnitData> grids;
            [ReadOnly]
            public ComponentDataFromEntity<DotGrid<TGrid>.ParentAreaData> parents;
            [ReadOnly]
            public ComponentDataFromEntity<PhysicsCollider> colliders;

            [ReadOnly]
            public ComponentDataFromEntity<DotGridArea.LinkToGridData> areas;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> poss;

            //[ReadOnly][NativeDisableUnsafePtrRestriction]
            //public uint *pDefualtBlankGrid;

            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute(int index)
            {
                var ent = this.KeyEntities[index];
                var grid = this.grids[ent];
                var parent = this.parents[ent];
                var pos = this.poss[ent];
                var area = this.areas[parent.ParentArea];

                var mesh = makeMesh_(in grid, in pos, in area, in this.mcdata);

                if (this.colliders.HasComponent(ent))
                {
                    this.colliders[ent].Value.Dispose();
                }

                cmd.AddComponent(index, ent, new PhysicsCollider
                {
                    Value = mesh,
                });
            }
        }

        static unsafe BlobAssetReference<Collider> makeTestCube_(Translation pos)
        {
            var geom = new BoxGeometry
            {
                Center = 0.0f,//pos.Value,
                Size = 32.0f,
                Orientation = quaternion.identity,
                BevelRadius = 0.2f,
            };
            return BoxCollider.Create(geom);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe BlobAssetReference<Collider> makeMesh_(
            in DotGrid<TGrid>.UnitData grid, in Translation pos,
            in DotGridArea.LinkToGridData grids,
            in BlobAssetReference<MarchingCubesBlobAsset> mcdata)
        {
            var writer = new MakeCube.MeshWriter
            {
                center = new float3(-16, 16, 16),
                vtxs = new NativeList<float3>(32 * 32 * 32 * 12 / 2, Allocator.Temp),
                tris = new NativeList<int3>(32 * 32 * 32 * 12 / 2, Allocator.Temp),
                filter = new CollisionFilter
                {
                    BelongsTo = 1 << 22,
                    CollidesWith = 0xffff_ffff,
                },
                mcdata = mcdata,
            };
            var near = grids.PickupNearGridIds(grid);

            MakeCube.SampleAllCubes(in near, ref writer);
            var mesh = writer.CreateMesh();
            //var mesh = makeTestCube_(pos);
            writer.Dispose();
            return mesh;
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


        //public struct CubeColliderUnits
        //{
        //    public BlobArray<BlobAssetReference<MeshCollider>> Units;
        //}
        //static unsafe BlobAssetReference<Collider> makeUnitCubeColliders_(Translation pos)
        //{




        //    return BoxCollider.Create(geom);
        //}
    }

    static class iex
    {
        public static int3 xxx(this int i) => new int3(i, i, i);
    }

}
