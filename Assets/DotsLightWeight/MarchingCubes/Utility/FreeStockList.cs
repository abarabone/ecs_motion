//using System.Collections;
//using System.Collections.Generic;
//using System;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;
//using UnityEngine;
//using Unity.Collections;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Burst;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.MarchingCubes
//{

//    public unsafe struct FreeStockList : IDisposable
//    {
//        [NativeDisableUnsafePtrRestriction]
//        DoubleSideStack<UIntPtr>* stocksPtr;


//        public FreeStockList(int maxBufferLength)
//        {
//            var p = (DoubleSideStack<UIntPtr>*)UnsafeUtility
//                .Malloc(sizeof(DoubleSideStack<UIntPtr>), UnsafeUtility.AlignOf<DoubleSideStack<UIntPtr>>(), Allocator.Persistent);
//            *p = new DoubleSideStack<UIntPtr>(maxBufferLength);

//            this.stocksPtr = p;
//        }

//        public void Dispose()
//        {
//            while (this.stocksPtr->PopFromBack(out var p)) DotGridAllocater.Dispose(p);
//            while (this.stocksPtr->PopFromFront(out var p)) DotGridAllocater.Dispose(p);
//            this.stocksPtr->Dispose();
//            UnsafeUtility.Free(this.stocksPtr, Allocator.Persistent);
//        }

//        public DotGrid32x32x32Unsafe Rent(GridFillMode fillMode)
//        {
//            switch (fillMode)
//            {
//                case GridFillMode.Blank:
//                    {
//                        var p = this.stocksPtr->PopFromFront();
//                        if (p != default) return new DotGrid32x32x32Unsafe(p, 0);

//                        var p_ = this.stocksPtr->PopFromBack();
//                        if (p_ != default) return DotGridAllocater.Fill(p_, fillMode);

//                        return DotGridAllocater.Alloc(fillMode);
//                    }
//                case GridFillMode.Solid:
//                    {
//                        var p = this.stocksPtr->PopFromBack();
//                        if (p != default) return new DotGrid32x32x32Unsafe(p, 32 * 32 * 32);

//                        var p_ = this.stocksPtr->PopFromFront();
//                        if (p_ != default) return DotGridAllocater.Fill(p_, fillMode);

//                        return DotGridAllocater.Alloc(fillMode);
//                    }
//                default:
//                    return new DotGrid32x32x32Unsafe();
//            }
//        }

//        public unsafe void Back(DotGrid32x32x32Unsafe grid, GridFillMode fillMode)
//        {
//            var isBackSuccess = fillMode switch
//            {
//                GridFillMode.Blank => this.stocksPtr->PushToFront((UIntPtr)grid.pXline),
//                GridFillMode.Solid => this.stocksPtr->PushToBack((UIntPtr)grid.pXline),
//                _ => true
//            };
//            if (!isBackSuccess) DotGridAllocater.Dispose((UIntPtr)grid.pXline);
//        }
//        public void Back(DotGrid32x32x32Unsafe grid) => Back(grid, grid.FillModeBlankOrSolid);
//    }

//}