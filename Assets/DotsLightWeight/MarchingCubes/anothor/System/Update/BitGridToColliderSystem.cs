//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Collections.LowLevel.Unsafe;
//using System;
//using Unity.Physics;
//using Unity.Jobs.LowLevel.Unsafe;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;

//namespace DotsLite.MarchingCubes
//{
//    using DotsLite.Dependency;
//    using DotsLite.Collision;

//    //[DisableAutoCreation]
//    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
//    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]// çƒçlÇÃïKóvÇ†ÇË
//    [UpdateAfter(typeof(DotGridUpdateSystem))]
//    public class DotGridToCollisionSystem : DependencyAccessableSystemBase
//    {

//        CommandBufferDependency.Sender cmddep;

//        BarrierDependency.Sender barfreedep;

//        DotGridMessageAllocSystem messageSystem;




//        protected override void OnCreate()
//        {
//            base.OnCreate();

//            this.RequireSingletonForUpdate<Global.CommonData>();

//            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
//            this.barfreedep = BarrierDependency.Sender.Create<DotGridMessageFreeSystem>(this);

//            this.messageSystem = this.World.GetOrCreateSystem<DotGridMessageAllocSystem>();
//        }

//        protected unsafe override void OnUpdate()
//        {
//            using var cmdscope = this.cmddep.WithDependencyScope();
//            using var barScope = this.barfreedep.WithDependencyScope();

//            this.Dependency = new JobExecution
//            {
//                cmd = cmdscope.CommandBuffer.AsParallelWriter(),
//                KeyEntities = this.messageSystem.Reciever.Holder.keyEntities.AsDeferredJobArray(),
//                mcdata = this.GetSingleton<Global.CommonData>().Assset,
//                //pDefualtBlankGrid = this.GetSingleton<Global.MainData<TGrid>>().DefaultGrids[(int)GridFillMode.Blank].pXline,
//                grids32 = this.GetComponentDataFromEntity<DotGrid.Unit32Data>(isReadOnly: true),
//                grids16 = this.GetComponentDataFromEntity<DotGrid.Unit16Data>(isReadOnly: true),
//                indexs = this.GetComponentDataFromEntity<DotGrid.IndexData>(isReadOnly: true),
//                parents = this.GetComponentDataFromEntity<DotGrid.ParentAreaData>(isReadOnly: true),
//                poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true),
//                areas = this.GetComponentDataFromEntity<DotGridArea.LinkToGridData>(isReadOnly: true),
//                colliders = this.GetComponentDataFromEntity<PhysicsCollider>(isReadOnly: true),
//            }
//            .Schedule(this.messageSystem.Reciever.Holder.keyEntities, 1, this.Dependency);
//        }



//        [BurstCompile]
//        unsafe struct JobExecution : IJobParallelForDefer
//        {
//            public EntityCommandBuffer.ParallelWriter cmd;

//            [ReadOnly]
//            public NativeArray<Entity> KeyEntities;
//            [ReadOnly]
//            public BlobAssetReference<MarchingCubesBlobAsset> mcdata;

//            [ReadOnly]
//            public ComponentDataFromEntity<DotGrid.Unit32Data> grids32;
//            [ReadOnly]
//            public ComponentDataFromEntity<DotGrid.Unit16Data> grids16;
//            [ReadOnly]
//            public ComponentDataFromEntity<DotGrid.IndexData> indexs;
//            [ReadOnly]
//            public ComponentDataFromEntity<DotGrid.ParentAreaData> parents;
//            [ReadOnly]
//            public ComponentDataFromEntity<PhysicsCollider> colliders;

//            [ReadOnly]
//            public ComponentDataFromEntity<DotGridArea.LinkToGridData> areas;
//            [ReadOnly]
//            public ComponentDataFromEntity<Translation> poss;

//            //[ReadOnly][NativeDisableUnsafePtrRestriction]
//            //public uint *pDefualtBlankGrid;

//            [BurstCompile]
//            [MethodImpl(MethodImplOptions.AggressiveInlining)]
//            public void Execute(int i)
//            {
//                var ent = this.KeyEntities[i];
//                var index = this.indexs[ent];
//                var parent = this.parents[ent];
//                var pos = this.poss[ent];
//                var area = this.areas[parent.ParentArea];

//                if (this.grids32.HasComponent(ent))
//                {
//                    var grid = this.grids32[ent];

//                    var mesh = makeMesh_(grid.Unit, in index, in pos, in area, in this.mcdata);

//                    if (this.colliders.HasComponent(ent))
//                    {
//                        this.colliders[ent].Value.Dispose();
//                    }

//                    cmd.AddComponent(i, ent, new PhysicsCollider
//                    {
//                        Value = mesh,
//                    });

//                    return;
//                }

//                if (this.grids16.HasComponent(ent))
//                {
//                    var grid = this.grids16[ent];

//                    //var mesh = makeMesh_(in grid.Unit, in index, in pos, in area, in this.mcdata);

//                    //if (this.colliders.HasComponent(ent))
//                    //{
//                    //    this.colliders[ent].Value.Dispose();
//                    //}

//                    //cmd.AddComponent(i, ent, new PhysicsCollider
//                    //{
//                    //    Value = mesh,
//                    //});

//                    return;
//                }
//            }
//        }

//        static unsafe BlobAssetReference<Collider> makeTestCube_(Translation pos)
//        {
//            var geom = new BoxGeometry
//            {
//                Center = 0.0f,//pos.Value,
//                Size = 32.0f,
//                Orientation = quaternion.identity,
//                BevelRadius = 0.2f,
//            };
//            return BoxCollider.Create(geom);
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        static unsafe BlobAssetReference<Collider> makeMesh_<TGrid>(
//            TGrid grid,
//            in DotGrid.IndexData index,
//            in Translation pos,
//            in DotGridArea.LinkToGridData grids,
//            in BlobAssetReference<MarchingCubesBlobAsset> mcdata)
//            where TGrid : struct, IDotGrid<TGrid>
//        {
//            var writer = new MakeCube.MeshWriter
//            {
//                center = new float3(-16, 16, 16),
//                vtxs = new NativeList<float3>(32 * 32 * 32 * 12 / 2, Allocator.Temp),
//                tris = new NativeList<int3>(32 * 32 * 32 * 12 / 2, Allocator.Temp),
//                filter = new CollisionFilter
//                {
//                    BelongsTo = 1 << 22,
//                    CollidesWith = 0xffff_ffff,
//                },
//                mcdata = mcdata,
//            };
//            var near = grids.PickupNearGridIds(in grid, in index);

//            MakeCube.SampleAllCubes(in near, ref writer);
//            var mesh = writer.CreateMesh();
//            //var mesh = makeTestCube_(pos);
//            writer.Dispose();
//            return mesh;
//        }

//        static unsafe void makeCubes_(uint *p)
//        {
//            for (var i = 0; i < 32 * 32; i++)
//            {
//                var xline = p[i];
//                for (var ix = 0; ix < 32; ix++)
//                {
//                    var iz = i & 0x1f;
//                    var iy = i >> 5 & 0x1f;



//                    xline >>= 1;
//                }
//            }
//            //for (var iy = 0; iy < 32; iy++)
//            //{
//            //    for (var iz = 0; iz < 32; iz++)
//            //    {
//            //        var xline = p[];
//            //        for (var ix = 0; ix < 32; ix++)
//            //        {
//            //        }
//            //    }
//            //}
//            //for (var i = 0; i < 32 * 32 * 32; i++)
//            //{
//            //    //var i3 = i.xxx() >> new int3(0, 10, 5) & 0x1f.xxx();
//            //    var ix = i & 0x1f;
//            //    var iz = i >> 5 & 0x1f;
//            //    var iy = i >> 10 & 0x1f;
//            //    p[i >> 5];
//            //}
//        }


//        //public struct CubeColliderUnits
//        //{
//        //    public BlobArray<BlobAssetReference<MeshCollider>> Units;
//        //}
//        //static unsafe BlobAssetReference<Collider> makeUnitCubeColliders_(Translation pos)
//        //{




//        //    return BoxCollider.Create(geom);
//        //}
//    }

//    static class iex
//    {
//        public static int3 xxx(this int i) => new int3(i, i, i);
//    }

//}
