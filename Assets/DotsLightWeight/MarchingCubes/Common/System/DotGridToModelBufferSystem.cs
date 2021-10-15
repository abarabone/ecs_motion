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

namespace DotsLite.Draw
{
    using DotsLite.Dependency;
    using MarchingCubes;

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Render.Draw.Transfer ) )]
    public class DotGridToModelBufferSystem : DependencyAccessableSystemBase
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


            var gridAreas = this.GetComponentDataFromEntity<DotGridArea.LinkToGridData>(isReadOnly: true);

            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>(isReadOnly: true);
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.VectorIndexData>( isReadOnly: true );

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithReadOnly(nativeBuffers)
                .WithReadOnly(gridAreas)
                .ForEach(
                    (
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Translation pos,
                        in DotGrid.UnitData data,
                        in DotGrid.ParentAreaData parent
                    ) =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        var lengthOfInstance = 1 + offsetInfo.OptionalVectorLengthPerInstance;
                        var i = target.DrawInstanceId * lengthOfInstance;// + offsetInfo.OptionalVectorLengthPerInstance;

                        var pModel = nativeBuffers[drawSysEnt].Transforms.pBuffer + offsetInfo.ModelStartIndex;

                        var ids = pickupNeargridIds_(gridAreas[parent.ParentArea], data.GridIndexInArea);
                        pModel[i + 0] = math.asfloat(ids.lPack4);
                        pModel[i + 1] = math.asfloat(ids.rPack4);// Debug.Log($"{ids.lPack4} {ids.rPack4}");

                        pModel[i + 2] = new float4(pos.Value, 1.0f);

                        return;


                        NearGridIndex pickupNeargridIds_(DotGridArea.LinkToGridData area, DotGrid.GridIndex index)
                        {
                            var ids = new NearGridIndex();

                            var p = area.pGridIds;
                            var span = area.GridSpan;


                            var lhome = index;
                            ids.left.home = p[lhome.serial];

                            var lrear = index.CloneNear(new int3(0, 0, 1), span);
                            ids.left.rear = p[lrear.serial];

                            var ldown = index.CloneNear(new int3(0, 1, 0), span);
                            ids.left.down = p[ldown.serial];

                            var lslant = index.CloneNear(new int3(0, 1, 1), span);
                            ids.left.slant = p[lslant.serial];


                            var rhome = index.CloneNear(new int3(1, 0, 0), span);
                            ids.right.home = p[rhome.serial];

                            var rrear = index.CloneNear(new int3(1, 0, 1), span);
                            ids.right.rear = p[rrear.serial];

                            var rdown = index.CloneNear(new int3(1, 1, 0), span);
                            ids.right.down = p[rdown.serial];

                            var rslant = index.CloneNear(new int3(1, 1, 1), span);
                            ids.right.slant = p[rslant.serial];


                            return ids;
                        }
                    }
                )
                .ScheduleParallel();
        }



    }

}
