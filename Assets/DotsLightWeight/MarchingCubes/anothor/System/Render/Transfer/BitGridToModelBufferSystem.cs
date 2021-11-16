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
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace DotsLite.Draw
{
    using DotsLite.Dependency;
    using MarchingCubes.another;
    using MarchingCubes.another.Data;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transfer))]
    public class BitGridToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawBufferToShaderDataSystem>(this);
        }


        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            var gridAreas = this.GetComponentDataFromEntity<BitGridArea.GridInstructionIdData>(isReadOnly: true);
            var gridDims = this.GetComponentDataFromEntity<BitGridArea.UnitDimensionData>(isReadOnly: true);

            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(gridAreas)
                .WithReadOnly(gridDims)
                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in Translation pos,
                    in BitGrid.GridTypeData unit,
                    in BitGrid.LocationInAreaData index,
                    in BitGrid.ParentAreaData parent) =>
                {
                    if (target.DrawInstanceId == -1) return;


                    var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                    var lengthOfInstance = 1 + offsetInfo.OptionalVectorLengthPerInstance;
                    var i = target.DrawInstanceId * lengthOfInstance;// + offsetInfo.OptionalVectorLengthPerInstance;

                    var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;

                    var pent = parent.ParentAreaEntity;
                    var ids = pickupNeargridIds_(gridAreas[pent], gridDims[pent].GridSpan.xyz, index.IndexInArea);
                    pModel[i + 0] = math.asfloat(ids.lPack4);
                    pModel[i + 1] = math.asfloat(ids.rPack4);// Debug.Log($"{ids.lPack4} {ids.rPack4}");

                    var u = unit.UnitOnEdge >> 1;
                    pModel[i + 2] = new float4(pos.Value - new float3(u, -u, -u), 1.0f);


                })
                .ScheduleParallel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe NearGridIndex pickupNeargridIds_(BitGridArea.GridInstructionIdData gids, int3 span, BitGrid.Tools.IndexInArea index)
        {
            var ids = new NearGridIndex();

            var p = gids.pId3dArray;


            var lhome = index;
            ids.left.home = p[lhome.serial];

            var lrear = index.Create(new int3(0, 0, 1), span);
            ids.left.rear = p[lrear.serial];

            var ldown = index.Create(new int3(0, 1, 0), span);
            ids.left.down = p[ldown.serial];

            var lslant = index.Create(new int3(0, 1, 1), span);
            ids.left.slant = p[lslant.serial];


            var rhome = index.Create(new int3(1, 0, 0), span);
            ids.right.home = p[rhome.serial];

            var rrear = index.Create(new int3(1, 0, 1), span);
            ids.right.rear = p[rrear.serial];

            var rdown = index.Create(new int3(1, 1, 0), span);
            ids.right.down = p[rdown.serial];

            var rslant = index.Create(new int3(1, 1, 1), span);
            ids.right.slant = p[rslant.serial];


            return ids;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GridInstraction
    {
        public float3 position;
        public int GridDynamicIndex;
        public NearGridIndex GridStaticIndex;
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct NearGridIndex
    {
        [FieldOffset(0)] public GridindexUnit left;
        [FieldOffset(16)] public GridindexUnit right;

        [FieldOffset(0)] public int4 lPack4;
        [FieldOffset(16)] public int4 rPack4;
    }
    public struct GridindexUnit
    {
        public int home;
        public int rear;
        public int down;
        public int slant;
    }
    // left_home;   // 0, 0, 0
    // left_rear;   // 0, 0, 1
    // left_down;   // 0, 1, 0
    // left_slant;  // 0, 1, 1
    // right_home;  // 1, 0, 0
    // right_rear;  // 1, 0, 1
    // right_down;  // 1, 1, 0
    // right_slant; // 1, 1, 1

}
