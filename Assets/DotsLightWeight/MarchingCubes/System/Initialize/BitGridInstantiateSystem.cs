using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Transforms;
using System.Runtime.CompilerServices;

namespace DotsLite.MarchingCubes
{
    using DotsLite.MarchingCubes.Data;
    using DotsLite.Draw;
    using DotsLite.Dependency;

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(DotGridLinksInitializeSystem))]
    [UpdateAfter(typeof(ResourceInitializeSystem))]
    public class BitGridInstantiateSystem : SystemBase
    {

        protected override unsafe void OnUpdate()
        {
            var em = this.EntityManager;

            this.Entities
                .WithName("GridArea")
                .WithoutBurst()
                .WithStructuralChanges()
                .WithAll<BitGridArea.InitializeData>()
                .ForEach((
                    Entity entity,
                    ref BitGridArea.GridInstructionIdData gids,
                    ref BitGridArea.GridLinkData glinks,
                    ref BitGridArea.GridInstructionIdSeedData seed,
                    in BitGridArea.GridTypeData type,
                    in BitGridArea.UnitDimensionData dim,
                    in Translation pos,
                    in BitGridArea.BitGridPrefabData prefab) =>
                {
                    create_(em, entity, new int3(0, 0, 0), ref glinks, ref gids, ref seed, in type, in dim, in prefab, in pos);
                    //create_(em, pfab, new int3(1, 0, 0), t, blen, span, ref glinks, ref gids, ref seed, basepos, entity, drawmdl);
                    //create_(em, pfab, new int3(0, 0, 1), t, blen, span, ref glinks, ref gids, ref seed, basepos, entity, drawmdl);
                    //create_(em, pfab, new int3(1, 0, 1), t, blen, span, ref glinks, ref gids, ref seed, basepos, entity, drawmdl);
                })
                .Run();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void create_(EntityManager em, Entity parent, int3 i,
            ref BitGridArea.GridLinkData glinks,
            ref BitGridArea.GridInstructionIdData gids,
            ref BitGridArea.GridInstructionIdSeedData seed,
            in BitGridArea.GridTypeData type,
            in BitGridArea.UnitDimensionData dim,
            in BitGridArea.BitGridPrefabData prefab,
            in Translation basepos)
        {
            var index = new BitGrid.Tools.IndexInArea(i, dim.GridSpan.xyz);

            var newent = em.Instantiate(prefab.Prefab);

            gids.pId3dArray[index.serial] = seed.NextId++;
            glinks.pGrid3dArray[index.serial] = newent;

            var u = type.UnitOnEdge;
            var hf = u >> 1;
            var pos = basepos.Value + (i * u + new int3(hf, -hf, -hf));

            em.SetComponentData(newent, new BitGrid.BitLinesData().Alloc(prefab.BitLineBufferLength, GridFillMode.Blank));
            em.SetComponentData(newent, new BitGrid.LocationInAreaData
            {
                IndexInArea = index,
                span = dim.GridSpan.xyz,
                BitBfufferOffset = prefab.BitLineBufferOffset + index.serial * prefab.BitLineBufferLength,
            });
            em.SetComponentData(newent, new DrawInstance.WorldBbox
            {
                Bbox = new AABB
                {
                    Center = pos,
                    Extents = hf,
                }
            });
            em.SetComponentData(newent, new Translation
            {
                Value = pos,
            });
            em.SetComponentData(newent, new BitGrid.ParentAreaData
            {
                ParentAreaEntity = parent,
            });
            em.SetComponentData(newent, new DrawInstance.ModelLinkData
            {
                DrawModelEntityCurrent = prefab.DrawModelEntity,
            });
        }

        protected override void OnDestroy()
        {
            this.Entities
                .WithoutBurst()
                .ForEach((ref BitGrid.BitLinesData lines) =>
                {
                    lines.Dispose();
                })
                .Run();
        }
    }

    // テストのためとりあえずグリッドを追加する
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(BitGridInstantiateSystem))]
    [UpdateAfter(typeof(BitGridMessageAllocSystem))]
    public class BitGridAddSystem : DependencyAccessableSystemBase
    {

        HitMessage<UpdateMessage>.Sender mcSender;

        //BarrierDependency.Sender bardep;


        protected override void OnCreate()
        {
            base.OnCreate();

            //this.bardep = BarrierDependency.Sender.Create<DotGridUpdateSystem>(this);
            //this.bardep = BarrierDependency.Sender.Create<DotGridMessageFreeSystem>(this);

            this.mcSender = HitMessage<UpdateMessage>.Sender.Create<BitGridMessageAllocSystem>(this);
        }

        protected override void OnUpdate()
        {
            //using var barscope = this.bardep.WithDependencyScope();

            using var mcScope = this.mcSender.WithDependencyScope();
            var w = mcScope.MessagerAsParallelWriter;

            this.Entities
                .WithAll<BitGrid.BitLinesData>()
                .ForEach((
                    Entity entity) =>
                {
                    w.Add(entity, new UpdateMessage
                    {
                        type = BitGridUpdateType.cube_force32,
                    });
                })
                .ScheduleParallel();

            this.Enabled = false;
        }
    }

}
