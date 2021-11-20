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
    using MarchingCubes;
    using MarchingCubes.Data;


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
            //var gridDims = this.GetComponentDataFromEntity<BitGridArea.UnitDimensionData>(isReadOnly: true);

            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(gridAreas)
                //.WithReadOnly(gridDims)
                .ForEach((
                    in DrawInstance.TargetWorkData target,
                    in DrawInstance.ModelLinkData linker,
                    in Translation pos,
                    in BitGrid.GridTypeData unit,
                    in BitGrid.LocationInAreaData locate,
                    in BitGrid.ParentAreaData parent) =>
                {
                    if (target.DrawInstanceId == -1) return;


                    var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                    var lengthOfInstance = 1 + offsetInfo.OptionalVectorLengthPerInstance;
                    var i = target.DrawInstanceId * lengthOfInstance;// + offsetInfo.OptionalVectorLengthPerInstance;

                    var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;

                    var pent = parent.ParentAreaEntity;
                    var ids = pickupNeargridIds_(gridAreas[pent], locate);
                    pModel[i + 0] = math.asfloat(ids.lPack4);
                    pModel[i + 1] = math.asfloat(ids.rPack4); Debug.Log($"{ids.lPack4} {ids.rPack4}");

                    var u = unit.UnitOnEdge >> 1;
                    pModel[i + 2] = new float4(pos.Value - new float3(u, -u, -u), 1.0f);


                })
                .ScheduleParallel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe NearGridIndex pickupNeargridIds_(in BitGridArea.GridInstructionIdData gids, in BitGrid.LocationInAreaData locate)
        {
            var span = locate.span.xyz;
            var index = locate.IndexInArea;

            var p = gids.pId3dArray;


            return new NearGridIndex
            {
                left = new GridindexUnit
                {
                    home = p[index.serial],
                    rear = p[index.Create(new int3(0, 0, 1), span).serial],
                    down = p[index.Create(new int3(0, 1, 0), span).serial],
                    slant= p[index.Create(new int3(0, 0, 1), span).serial],
                },
                right = new GridindexUnit
                {
                    home = p[index.Create(new int3(1, 0, 0), span).serial],
                    rear = p[index.Create(new int3(1, 0, 1), span).serial],
                    down = p[index.Create(new int3(1, 1, 0), span).serial],
                    slant= p[index.Create(new int3(1, 0, 1), span).serial],
                }
            };
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
