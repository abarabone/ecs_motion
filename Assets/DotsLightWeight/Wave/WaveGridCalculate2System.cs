using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Burst.Intrinsics;

namespace DotsLite.HeightGrid
{
    using DotsLite.Misc;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMove))]
    public class WaveGridCalculate2System : SystemBase
    {

        GridMaster.Data gridMaster;


        protected override void OnStartRunning()
        {
            this.RequireSingletonForUpdate<GridMaster.Data>();

            if (!this.HasSingleton<GridMaster.Data>()) return;
            this.gridMaster = this.GetSingleton<GridMaster.Data>();
            Debug.Log(this.gridMaster);

            this.gridMaster.Currs[10] = -3f;
            this.gridMaster.Currs[11] = -3f;
            this.gridMaster.Currs[12] = -3f;
            this.gridMaster.Currs[13] = -3f;
            this.gridMaster.Currs[14] = -3f;
            this.gridMaster.Currs[15] = -3f;
            this.gridMaster.Currs[16] = -3f;
            this.gridMaster.Currs[17] = -3f;
            this.gridMaster.Currs[18] = -3f;
            this.gridMaster.Currs[19] = -3f;
        }


        protected override unsafe void OnUpdate()
        {
            var dt = this.Time.DeltaTime * 5;
            var dtrate = dt * TimeEx.PrevDeltaTimeRcp;
            var sqdt = dt * dt;
            var harfsqdt = 0.5f * sqdt;

            var gridinfo = this.gridMaster.Info;
            var spanInGrid = gridinfo.UnitLengthInGrid;
            spanInGrid.x >>= 2;
            var span = gridinfo.NumGrids * spanInGrid;
            var totalLen = span.x * span.y;
            var lenInGrid = spanInGrid.x * spanInGrid.y;

            var maskInGrid = spanInGrid - 1;
            var mask = span - 1;

            var nexts = this.gridMaster.Nexts;
            var currs = this.gridMaster.Currs;
            var prevs = this.gridMaster.Prevs;
            var d = gridinfo.Dumping;
            var c2 = gridinfo.Constraint2;

            //var pNextsRo = (float4*)this.gridMaster.Nexts.GetUnsafeReadOnlyPtr();
            //var pCurrsRw = (float4*)this.gridMaster.Currs.GetUnsafePtr();
            //var pPrevsRw = (float4*)this.gridMaster.Prevs.GetUnsafePtr();

            this.Entities
                .WithName("copy")
                .WithBurst()
                //.WithNativeDisableUnsafePtrRestriction(pNextsRo)
                //.WithNativeDisableUnsafePtrRestriction(pCurrsRw)
                //.WithNativeDisableUnsafePtrRestriction(pPrevsRw)
                .WithNativeDisableParallelForRestriction(nexts)
                .WithNativeDisableParallelForRestriction(currs)
                .WithNativeDisableParallelForRestriction(prevs)
                .WithNativeDisableContainerSafetyRestriction(nexts)
                .WithNativeDisableContainerSafetyRestriction(currs)
                .WithNativeDisableContainerSafetyRestriction(prevs)
                .WithReadOnly(nexts)
                .WithAll<Height.GridLevel0Tag>()
                .ForEach((in Height.GridData grid) =>
                {

                    var pNextsRo = (float4*)nexts.GetUnsafeReadOnlyPtr();
                    var pCurrsRw = (float4*)currs.GetUnsafePtr();
                    var pPrevsRw = (float4*)prevs.GetUnsafePtr();

                    var ofs = grid.GridId * spanInGrid;

                    for (var ig = 0; ig < lenInGrid >> 2; ig++)
                    {
                        var ig2 = ig << 2;
                        var i = ofs + new int2(ig2 & maskInGrid.x, ig2 >> math.countbits(maskInGrid.x));
                        var index = i.x + i.y * span.x;
                        pPrevsRw[index + 0] = pCurrsRw[index + 0];
                        pCurrsRw[index + 0] = pNextsRo[index + 0];
                        pPrevsRw[index + 1] = pCurrsRw[index + 1];
                        pCurrsRw[index + 1] = pNextsRo[index + 1];
                        pPrevsRw[index + 2] = pCurrsRw[index + 2];
                        pCurrsRw[index + 2] = pNextsRo[index + 2];
                        pPrevsRw[index + 3] = pCurrsRw[index + 3];
                        pCurrsRw[index + 3] = pNextsRo[index + 3];
                    }


                    //var ofs = grid.GridId * spanInGrid;
                    //var i = ofs.x + ofs.y * span.x;

                    //{
                    //    var pCurrsRo = (float4*)currs.GetUnsafeReadOnlyPtr();
                    //    var pPrevsRw = (float4*)prevs.GetUnsafePtr();
                    //    var pDst = pPrevsRw + i;
                    //    var pSrc = pCurrsRo + i;
                    //    var outspan = span.x * sizeof(float4);
                    //    var elmspan = spanInGrid.x * sizeof(float4);
                    //    var count = spanInGrid.y;
                    //    UnsafeUtility.MemCpyStride(pDst, outspan, pSrc, outspan, elmspan, count);
                    //}
                    //{
                    //    var pNextsRo = (float4*)nexts.GetUnsafeReadOnlyPtr();
                    //    var pCurrsRw = (float4*)currs.GetUnsafePtr();
                    //    var pDst = pCurrsRw + i;
                    //    var pSrc = pNextsRo + i;
                    //    var outspan = span.x * sizeof(float4);
                    //    var elmspan = spanInGrid.x * sizeof(float4);
                    //    var count = spanInGrid.y;
                    //    UnsafeUtility.MemCpyStride(pDst, outspan, pSrc, outspan, elmspan, count);
                    //}

                })
                .ScheduleParallel();

            //var pNextsRw = (float4*)this.gridMaster.Nexts.GetUnsafePtr();
            //var pCurrsRo = (float4*)this.gridMaster.Currs.GetUnsafeReadOnlyPtr();
            //var pPrevsRo = (float4*)this.gridMaster.Prevs.GetUnsafeReadOnlyPtr();

            this.Entities
                .WithName("move")
                .WithBurst()
                //.WithNativeDisableUnsafePtrRestriction(pNextsRw)
                //.WithNativeDisableUnsafePtrRestriction(pCurrsRo)
                //.WithNativeDisableUnsafePtrRestriction(pPrevsRo)
                //.WithReadOnly(pCurrsRo)
                //.WithReadOnly(pPrevsRo)
                .WithReadOnly(prevs)
                .WithReadOnly(currs)
                .WithNativeDisableParallelForRestriction(nexts)
                .WithNativeDisableParallelForRestriction(currs)
                .WithNativeDisableParallelForRestriction(prevs)
                .WithNativeDisableContainerSafetyRestriction(nexts)
                .WithNativeDisableContainerSafetyRestriction(currs)
                .WithNativeDisableContainerSafetyRestriction(prevs)
                .WithAll<Height.GridLevel0Tag>()
                .ForEach((in Height.GridData grid) =>
                {

                    var pNextsRw = (float4*)nexts.GetUnsafePtr();
                    var pCurrsRo = (float4*)currs.GetUnsafeReadOnlyPtr();
                    var pPrevsRo = (float4*)prevs.GetUnsafeReadOnlyPtr();

                    var ofs = grid.GridId * spanInGrid;
    
                    for (var ig = 0; ig < lenInGrid; ig++)
                    {
                        //const float c2 = 0.8f;
                        //const float d = 0.999f;

                        var i = ofs + new int2(ig & maskInGrid.x, ig >> math.countbits(maskInGrid.x));
                        var index = i.x + i.y * span.x;

                        int h(int iy) => i.x + (iy & mask.y) * span.x;
                        int w(int ix) => (ix & mask.x) + i.y * span.x;


                        var hc = pCurrsRo[index];

                        var hu = pCurrsRo[h(i.y - 1)];
                        var hd = pCurrsRo[h(i.y + 1)];

                        var hl = hc.wxyz;
                        hl.x = pCurrsRo[w(i.x - 1)].w;
                        var hr = hc.yzwx;
                        hr.w = pCurrsRo[w(i.x + 1)].x;
                        //var hl = pCurrsRo[w(i.x - 1)];
                        //var hr = pCurrsRo[w(i.x + 1)];


                        var hc2 = hc + hc;
                        var aw = c2 * (hl + hr - hc2);
                        var ah = c2 * (hu + hd - hc2);

                        var hp = pPrevsRo[index];
                        var hn = hc + (hc - hp) * d + (aw + ah) * harfsqdt;

                        pNextsRw[index] = hn;
                    }

                })
                .ScheduleParallel();

        }
    }
}
