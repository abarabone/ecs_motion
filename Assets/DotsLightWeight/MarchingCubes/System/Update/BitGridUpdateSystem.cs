using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotsLite.MarchingCubes
{
    using DotsLite.Dependency;
    using DotsLite.MarchingCubes.Data;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    public class BitGridUpdateSystem : DependencyAccessableSystemBase
    {

        BarrierDependency.Sender barcopydep;
        //BarrierDependency.Sender barfreedep;

        BitGridMessageAllocSystem messageSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.barcopydep = BarrierDependency.Sender.Create<BitGridCopyToGpuSystem>(this);
            //this.barfreedep = BarrierDependency.Sender.Create<BitGridMessageFreeSystem>(this);

            this.messageSystem = this.World.GetOrCreateSystem<BitGridMessageAllocSystem>();
        }

        protected override void OnUpdate()
        {
            using var barcopyScope = this.barcopydep.WithDependencyScope();
            //using var barfreeScope = this.barfreedep.WithDependencyScope();

            this.Dependency = new JobExecution
            {
                bitgrids = this.GetComponentDataFromEntity<BitGrid.BitLinesData>(isReadOnly: true),
                dirties = this.GetComponentDataFromEntity<BitGrid.UpdateDirtyRangeData>(),
                parents = this.GetComponentDataFromEntity<BitGrid.ParentAreaData>(isReadOnly: true),
                origins = this.GetComponentDataFromEntity<BitGrid.WorldOriginData>(isReadOnly: true),
                areas = this.GetComponentDataFromEntity<BitGridArea.GridLinkData>(isReadOnly: true),
                dims = this.GetComponentDataFromEntity<BitGridArea.UnitDimensionData>(isReadOnly: true),
            }
            .ScheduleParallelKey(this.messageSystem.Reciever, 1, this.Dependency);
        }


        [BurstCompile]
        public struct JobExecution : HitMessage<UpdateMessage>.IApplyJobExecutionForKey
        {
            [ReadOnly]
            public ComponentDataFromEntity<BitGrid.BitLinesData> bitgrids;
            [ReadOnly]
            public ComponentDataFromEntity<BitGrid.ParentAreaData> parents;
            [ReadOnly]
            public ComponentDataFromEntity<BitGrid.WorldOriginData> origins;

            [ReadOnly]
            public ComponentDataFromEntity<BitGridArea.GridLinkData> areas;
            [ReadOnly]
            public ComponentDataFromEntity<BitGridArea.UnitDimensionData> dims;

            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<BitGrid.UpdateDirtyRangeData> dirties;


            [BurstCompile]
            public unsafe void Execute(
                int index, Entity targetEntity,
                NativeMultiHashMap<Entity, UpdateMessage>.Enumerator msgs)
            {
                var parent = this.parents[targetEntity].ParentAreaEntity;
                var origin = this.origins[targetEntity].Origin.xyz;
                var area = this.areas[parent];
                var dim = this.dims[parent];

                foreach (var msg in msgs)
                {
                    switch (msg.type)
                    {
                        case BitGridUpdateType.cube_force32:
                            {
                                //this.dotgrids[targetEntity].Unit.Fill();
                                var p = this.bitgrids[targetEntity].p;

                                //var v0 = 0u;
                                //for (var i = 0; i < 32; i++)
                                //{
                                //    p[i] = v0;
                                //}

                                //var v = 0xf444_4b44u;
                                //for (var i = 32; i < 32 * 31; i++)
                                //{
                                //    p[i] = v;
                                //    v = (v << 3) | (v >> 28);
                                //}

                                //for (var i = 32 * 31; i < 32 * 32; i++)
                                //{
                                //    p[i] = v0;
                                //}

                                for (var i = 32 * 2; i < 32 * 32; i++)
                                {
                                    p[i] = 0xffffffff;
                                }

                                this.dirties[targetEntity] = new BitGrid.UpdateDirtyRangeData
                                {
                                    begin = 32 * 1,//32 * 32 / 2,
                                    end = 32 * 32 -1,//dim.UnitOnEdge.z * dim.UnitOnEdge.y - 1,
                                };
                            }
                            break;
                        case BitGridUpdateType.point_remove32:
                            {
                                var localpos = (msg.point - origin) * new float3(1, -1, -1);
                                var localpos_clamped = math.clamp(localpos, float3.zero, dim.UnitOnEdge.xyz);
                                var hitIndex = (int3)math.floor(localpos_clamped + new float3(0.5f, 0.5f, 0.5f));

                                var iline = hitIndex.y * 32 + hitIndex.z;
                                var targetbit = 1u << hitIndex.x;
                                //UnityEngine.Debug.Log($"{msg.aabb.Center} {origin} {localpos} {hitIndex} {iline} {targetbit}");

                                var p = this.bitgrids[targetEntity].p;
                                p[iline] &= ~targetbit;

                                this.dirties[targetEntity] = new BitGrid.UpdateDirtyRangeData
                                {
                                    begin = iline,
                                    end = iline,
                                };
                            }
                            break;
                        case BitGridUpdateType.point_add32:
                            {
                                var localpos = (msg.point - origin) * new float3(1, -1, -1);
                                var localpos_clamped = math.clamp(localpos, float3.zero, dim.UnitOnEdge.xyz);
                                var hitIndex = (int3)math.floor(localpos_clamped + new float3(0.5f, 0.5f, 0.5f));

                                var iline = hitIndex.y * dim.UnitOnEdge.z + hitIndex.z;
                                var targetbit = 1u << hitIndex.x;
                                //UnityEngine.Debug.Log($"{msg.aabb.Center} {origin} {localpos} {hitIndex} {iline} {targetbit}");

                                var p = this.bitgrids[targetEntity].p;
                                p[iline] |= targetbit;//&= ~targetbit;

                                this.dirties[targetEntity] = new BitGrid.UpdateDirtyRangeData
                                {
                                    begin = iline,
                                    end = iline,
                                };
                            }
                            break;
                        case BitGridUpdateType.aabb_remove32:
                            {
                                var local0 = (msg.aabb.Center - msg.aabb.Extents - origin) * new float3(1, -1, -1);// - new float3(0.5f, 0.5f, 0.5f);
                                var local1 = (msg.aabb.Center + msg.aabb.Extents - origin) * new float3(1, -1, -1);// - new float3(0.5f, 0.5f, 0.5f);
                                var local0_clamped = math.clamp(local0, float3.zero, dim.UnitOnEdge.xyz);
                                var local1_clamped = math.clamp(local1, float3.zero, dim.UnitOnEdge.xyz);

                                var stlocal_clamped = math.min(local0_clamped, local1_clamped);
                                var edlocal_clamped = math.max(local0_clamped, local1_clamped);

                                var sthitIndex = (int3)math.floor(stlocal_clamped);
                                var edhitIndex = (int3)math.ceil(edlocal_clamped);

                                //UnityEngine.Debug.Log($"{stlocal_clamped} {edlocal_clamped} {sthitIndex} {edhitIndex}");
                                var p = this.bitgrids[targetEntity].p;
                                for (var iy = sthitIndex.y; iy <= edhitIndex.y; iy++)
                                    for (var iz = sthitIndex.z; iz <= edhitIndex.z; iz++)
                                    {
                                        var len = (edhitIndex.x - sthitIndex.x) + 1;
                                        var mask = ((1u << len) - 1) << sthitIndex.x;
                                        p[iy * dim.UnitOnEdge.z + iz] &= ~mask;
                                        //UnityEngine.Debug.Log($"{mask}");
                                    }

                                this.dirties[targetEntity] = new BitGrid.UpdateDirtyRangeData
                                {
                                    begin = sthitIndex.y * dim.UnitOnEdge.z + sthitIndex.z,
                                    end = edhitIndex.y * dim.UnitOnEdge.z + edhitIndex.z,
                                };
                            }
                            break;
                        case BitGridUpdateType.sphere_remove32:
                            {

                            }
                            break;
                        case BitGridUpdateType.capsule_remove32:

                            break;
                        default: break;
                    }
                }

            }
        }

    }

    public unsafe partial struct DotGrid32x32x32
    {
        public void Fill()
        {

        }
    }

    public unsafe partial struct DotGrid16x16x16
    {
        public void Fill()
        {

        }
    }
}
