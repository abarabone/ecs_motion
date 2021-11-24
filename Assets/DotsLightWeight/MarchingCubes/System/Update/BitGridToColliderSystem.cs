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
    using DotsLite.MarchingCubes.Data;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]// çƒçlÇÃïKóvÇ†ÇË
    [UpdateAfter(typeof(BitGridUpdateSystem))]
    public class BitGridToCollisionSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;

        BarrierDependency.Sender barfreedep;

        BitGridMessageAllocSystem messageSystem;




        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<Common.DrawShaderResourceData>();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.barfreedep = BarrierDependency.Sender.Create<BitGridMessageFreeSystem>(this);

            this.messageSystem = this.World.GetOrCreateSystem<BitGridMessageAllocSystem>();
        }

        protected unsafe override void OnUpdate()
        {
            using var cmdscope = this.cmddep.WithDependencyScope();
            using var barScope = this.barfreedep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                cmd = cmdscope.CommandBuffer.AsParallelWriter(),
                KeyEntities = this.messageSystem.Reciever.Holder.keyEntities.AsDeferredJobArray(),
                mcdata = this.GetSingleton<Common.AssetData>().Asset,

                gridtypes = GetComponentDataFromEntity<BitGrid.GridTypeData>(isReadOnly: true),
                grids = GetComponentDataFromEntity<BitGrid.BitLinesData>(isReadOnly: true),
                locates = GetComponentDataFromEntity<BitGrid.LocationInAreaData>(isReadOnly: true),
                parents = GetComponentDataFromEntity<BitGrid.ParentAreaData>(isReadOnly: true),
                colliders = GetComponentDataFromEntity<PhysicsCollider>(isReadOnly: true),

                linkss = GetComponentDataFromEntity<BitGridArea.GridLinkData>(isReadOnly: true),
                gidss = GetComponentDataFromEntity<BitGridArea.GridInstructionIdData>(isReadOnly: true),
                poss = GetComponentDataFromEntity<Translation>(isReadOnly: true),
            }
            .Schedule(this.messageSystem.Reciever.Holder.keyEntities, 1, this.Dependency);
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
            public ComponentDataFromEntity<BitGrid.GridTypeData> gridtypes;
            [ReadOnly]
            public ComponentDataFromEntity<BitGrid.BitLinesData> grids;
            [ReadOnly]
            public ComponentDataFromEntity<BitGrid.LocationInAreaData> locates;
            [ReadOnly]
            public ComponentDataFromEntity<BitGrid.ParentAreaData> parents;
            [ReadOnly]
            public ComponentDataFromEntity<PhysicsCollider> colliders;

            [ReadOnly]
            public ComponentDataFromEntity<BitGridArea.GridLinkData> linkss;
            [ReadOnly]
            public ComponentDataFromEntity<BitGridArea.GridInstructionIdData> gidss;
            [ReadOnly]
            public ComponentDataFromEntity<Translation> poss;

            //[ReadOnly][NativeDisableUnsafePtrRestriction]
            //public uint *pDefualtBlankGrid;

            [BurstCompile]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Execute(int i)
            {
                var ent = this.KeyEntities[i];
                var gtype = this.gridtypes[ent];
                var grid = this.grids[ent];
                var locate = this.locates[ent];
                var parent = this.parents[ent];
                var pos = this.poss[ent];
                var links = this.linkss[parent.ParentAreaEntity];
                var gids = this.gidss[parent.ParentAreaEntity];

                if (gtype.GridType == BitGridType.Grid32x32x32)
                {
                    //UnityEngine.Debug.Log(gtype.GridType);
                    var collider = makeMeshCollider_(
                        in grids, in links, in gids, in grid, in locate, in gtype, in mcdata);

                    if (this.colliders.HasComponent(ent))
                    {
                        this.colliders[ent].Value.Dispose();
                    }

                    cmd.AddComponent(i, ent, new PhysicsCollider
                    {
                        Value = collider,
                    });

                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        static unsafe BlobAssetReference<Collider> makeMeshCollider_(
            in ComponentDataFromEntity<BitGrid.BitLinesData> grids,
            in BitGridArea.GridLinkData links,
            in BitGridArea.GridInstructionIdData ids,
            in BitGrid.BitLinesData grid,
            in BitGrid.LocationInAreaData locate,
            in BitGrid.GridTypeData type,
            in BlobAssetReference<MarchingCubesBlobAsset> mcdata)
        {
            var u = type.UnitOnEdge.xyz;
            var writer = new MakeCube.MeshWriter
            {
                center = u * new float3(-0.5f, 0.5f, 0.5f),
                vtxs = new NativeList<float3>(u.x * u.y * u.z * 12 / 2, Allocator.Temp),
                tris = new NativeList<int3>(u.x * u.y * u.z * 12 / 2, Allocator.Temp),
                filter = new CollisionFilter
                {
                    BelongsTo = 1 << 22,
                    CollidesWith = 0xffff_ffff,
                },
                mcdata = mcdata,
            };
            var near = grids.PickupNearGridIds(in links, in ids, in grid, in locate);

            MakeCube.SampleAllCubes(in near, ref writer, u);
            var collider = writer.CreateMesh();
            //var mesh = makeTestCube_(pos);
            writer.Dispose();
            return collider;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void makeCubes_(uint* p)
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

    static class NearGridExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe MakeCube.NearBitGrids PickupNearGridIds(
            in this ComponentDataFromEntity<BitGrid.BitLinesData> grids,
            in BitGridArea.GridLinkData links, in BitGridArea.GridInstructionIdData ids,
            in BitGrid.BitLinesData grid, in BitGrid.LocationInAreaData locate)
        {

            var pGrid = links.pGrid3dArray;
            var pId = ids.pId3dArray;
            var index = locate.IndexInArea;


            var lhome = index;
            var elx = pGrid[lhome.serial];
            var ilx = pId[lhome.serial];

            var lrear = index.Create(0, 0, 1);
            var elz = pGrid[lrear.serial];
            var ilz = pId[lrear.serial];

            var ldown = index.Create(0, 1, 0);
            var ely = pGrid[ldown.serial];
            var ily = pId[ldown.serial];

            var lslant = index.Create(0, 1, 1);
            var elw = pGrid[lslant.serial];
            var ilw = pId[lslant.serial];


            var rhome = index.Create(1, 0, 0);
            var erx = pGrid[rhome.serial];
            var irx = pId[rhome.serial];

            var rrear = index.Create(1, 0, 1);
            var erz = pGrid[rrear.serial];
            var irz = pId[rrear.serial];

            var rdown = index.Create(1, 1, 0);
            var ery = pGrid[rdown.serial];
            var iry = pId[rdown.serial];

            var rslant = index.Create(1, 1, 1);
            var erw = pGrid[rslant.serial];
            var irw = pId[rslant.serial];


            //return new MakeCube.NearBitGrids
            //{
            //    L = new MakeCube.NearBitGrids.HalfGridUnit
            //    {
            //        isContained = (uint4)math.sign(new int4(ilx, ily, ilz, ilw) + 1),
            //        x = grids[elx].p,
            //        y = grids[ely].p,
            //        z = grids[elz].p,
            //        w = grids[elw].p,
            //    },
            //    R = new MakeCube.NearBitGrids.HalfGridUnit
            //    {
            //        isContained = (uint4)math.sign(new int4(irx, iry, irz, irw) + 1),
            //        x = grids[erx].p,
            //        y = grids[ery].p,
            //        z = grids[erz].p,
            //        w = grids[erw].p,
            //    },
            //};
            return new MakeCube.NearBitGrids
            {
                L = new MakeCube.NearBitGrids.HalfGridUnit
                {
                    isContained = (uint4)math.sign(new int4(ilx, ily, ilz, ilw) + 1),
                    x = elx != Entity.Null ? grids[elx].p : null,
                    y = ely != Entity.Null ? grids[ely].p : null,
                    z = elz != Entity.Null ? grids[elz].p : null,
                    w = elw != Entity.Null ? grids[elw].p : null,
                },
                R = new MakeCube.NearBitGrids.HalfGridUnit
                {
                    isContained = (uint4)math.sign(new int4(irx, iry, irz, irw) + 1),
                    x = erx != Entity.Null ? grids[erx].p : null,
                    y = ery != Entity.Null ? grids[ery].p : null,
                    z = erz != Entity.Null ? grids[erz].p : null,
                    w = erw != Entity.Null ? grids[erw].p : null,
                },
            };
        }
    }
}
