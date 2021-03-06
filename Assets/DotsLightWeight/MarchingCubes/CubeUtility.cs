﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    static public class CubeUtility
    {


        //[MethodImpl( MethodImplOptions.AggressiveInlining )]
        //static public bool4 IsEmptyOrFull( this uint4 ui4 ) => math.any( ui4 + 1 & 0xfe );

        //[MethodImpl( MethodImplOptions.AggressiveInlining )]
        //static public uint4 _0or255to0( this uint4 ui4 ) => ui4 + 1 & 0xfe;



        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static public int AsByte( this bool b ) => new BoolAsByte() { bl = b }.by;
        [StructLayout(LayoutKind.Explicit)]
        public struct BoolAsByte
        {
            [FieldOffset( 0 )] public bool bl;
            [FieldOffset( 0 )] public byte by;
        }




        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static public uint ToCubeInstance( int ix, int iy, int iz, int gridId, uint cubeId ) =>
            //(uint)iz << 24 | (uint)iy << 16 | (uint)ix << 8 | cubeId;
            //(uint)iz << 26 & 0x1fu << 26 | (uint)iy << 21 & 0x1fu << 21 | (uint)ix << 16 & 0x1fu << 16 | (uint)gridId << 8 & 0xffu << 8 | cubeId & 0xff;
            (uint)iz << 26 | (uint)iy << 21 | (uint)ix << 16 | (uint)gridId << 8 | cubeId;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static public (float3 center, uint gridId, uint cubeId) FromCubeInstance( uint cubeInstance ) =>
            //(new float3( cubeInstance >> 8 & 0xff, -( cubeInstance >> 16 & 0xff ), -( cubeInstance >> 24 & 0xff ) ), cubeInstance & 0xff);
            (new float3( cubeInstance >> 16 & 0x1f, -( cubeInstance >> 21 & 0x1f ), -( cubeInstance >> 26 & 0x1f ) ), cubeInstance >> 8 & 0xff, cubeInstance & 0xff);


        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static public uint4 ToCubeInstance( int4 ix, int4 iy, int4 iz, int gridId, uint4 cubeId ) =>
            (uint4)iz << 26 | (uint4)iy << 21 | (uint4)ix << 16 | (uint)gridId << 8 | cubeId;



        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static public uint PackToByte4Uint( int x, int y, int z, int w ) =>
            (uint)( (x & 0xff)<<0 | (y & 0xff)<<8 | (z & 0xff)<<16 | (w & 0xff)<<24 );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static public uint PackToByte4Uint( this (int x, int y, int z, int w) i ) => PackToByte4Uint( i.x, i.y, i.z, i.w );




        static public (NativeList<int> tris, NativeList<float3> vtxs) MakeCollisionMeshData
            ( IEnumerable<CubeInstance> cubeInstances, int[][] srcIdxLists, float3[] srcVtxList )
        {
            var dstIdxs = new NativeList<int>( 32*32*32*12 / 2, Allocator.Temp );
            var dstVtxs = new NativeList<float3>( 32*32*32*12 / 2, Allocator.Temp );

            var vtxOffset = 0;
            //for( var i = 0; i < cubeInstances.Length; i++ )
            foreach( var ci in cubeInstances )
            {
                vtxOffset = addCube_( ci.instance, vtxOffset );
            }

            return (dstIdxs, dstVtxs);


            int addCube_( uint cubeInstance, int vtxOffset_ )
            {
                //var cubeId = cubeInstance & 0xff;
                //if( cubeId == 0 || cubeId == 255 ) return vtxOffset_;

                //var center = new float3( cubeInstance >> 8 & 0xff, -( cubeInstance >> 16 & 0xff ), -( cubeInstance >> 24 & 0xff ) );

                var (center, gridId, cubeId) = CubeUtility.FromCubeInstance( cubeInstance );
                if( cubeId == 0 || cubeId == 255 ) return vtxOffset_;

                var srcIdxList = srcIdxLists[ cubeId - 1 ];

                for( var i = 0; i < srcIdxList.Length; i++ )
                {
                    var srcIdx = srcIdxList[ i ];
                    dstIdxs.Add( vtxOffset_ + srcIdx );
                }
                for( var i = 0; i < srcVtxList.Length; i++ )
                {
                    dstVtxs.Add( srcVtxList[ i ] + center );
                }

                return vtxOffset_ + srcVtxList.Length;
            }
        }



        //[MethodImpl( MethodImplOptions.AggressiveInlining )]
        //static public uint ToNearGridId( int3 nearGridId ) =>
        //    (uint)nearGridId.z << 18 | (uint)nearGridId.y << 9 | (uint)nearGridId.x << 0;


        static public void GetNearGridList
            (NativeArray<GridInstanceData> gridData, float3 gridScale)
        {

            var posDict = new NativeHashMap<float3, int>( gridData.Length, Allocator.Temp );

            var i_to_gridpos = gridScale * new float3(1,-1,-1);

            addToDict_();
            getNearGridIds_();

            posDict.Dispose();
            return;


            void addToDict_()
            {
                for( var i = 0; i < gridData.Length; i++ )
                {
                    var pos = gridData[ i ].Position;
                    posDict.Add( pos.xyz * i_to_gridpos, i );
                }
            }
            void getNearGridIds_()
            {
                for( var i = 0; i < gridData.Length; i++ )
                {
                    var data = gridData[ i ];
                    var pos = data.Position;

                    var currentpos = pos.xyz * i_to_gridpos;
                    posDict.TryGetValue( currentpos, out var currentId );

                    //data.current = (ushort)currentId;


                    var prevx = currentpos + new float3( -1,  0,  0 );
                    var prevy = currentpos + new float3(  0, -1,  0 );
                    var prevz = currentpos + new float3(  0,  0, -1 );

                    var prevId = new int3( -1, -1, -1 );// デフォルトを -1 にしようとしたが…

                    posDict.TryGetValue( prevx, out prevId.x ); // 失敗すると 0 が入るので、-1 は維持されない
                    posDict.TryGetValue( prevy, out prevId.y );
                    posDict.TryGetValue( prevz, out prevId.z );

                    //data.left = (ushort)prevId.x;
                    //data.up = (ushort)prevId.y;
                    //data.back = (ushort)prevId.z;


                    var nextx = currentpos + new int3( 1, 0, 0 );
                    var nexty = currentpos + new int3( 0, 1, 0 );
                    var nextz = currentpos + new int3( 0, 0, 1 );

                    var nextId = new int3( -1, -1, -1 );

                    posDict.TryGetValue( nextx, out nextId.x );
                    posDict.TryGetValue( nexty, out nextId.y );
                    posDict.TryGetValue( nextz, out nextId.z );

                    //data.right = (ushort)nextId.x;
                    //data.down = (ushort)nextId.y;
                    //data.forward = (ushort)nextId.z;


                    data.ortho.x = (uint)(prevId.z << 0 | prevId.y << 16);
                    data.ortho.y = (uint)(prevId.x << 0 | currentId << 16);
                    data.ortho.z = (uint)(nextId.x << 0 | nextId.y << 16);
                    data.ortho.w = (uint)(nextId.z << 0);

                    gridData[ i ] = data;
                }
            }
        }


    }




    public unsafe struct FreeStockList : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        DoubleSideStack<UIntPtr>* stocksPtr;


        public FreeStockList(int maxBufferLength)
        {
            var p = (DoubleSideStack<UIntPtr>*)UnsafeUtility
                .Malloc(sizeof(DoubleSideStack<UIntPtr>), UnsafeUtility.AlignOf<DoubleSideStack<UIntPtr>>(), Allocator.Persistent);
            *p = new DoubleSideStack<UIntPtr>(maxBufferLength);

            this.stocksPtr = p;
        }

        public void Dispose()
        {
            while (this.stocksPtr->PopFromBack(out var p)) DotGridAllocater.Dispose(p);
            while (this.stocksPtr->PopFromFront(out var p)) DotGridAllocater.Dispose(p);
            this.stocksPtr->Dispose();
            UnsafeUtility.Free(this.stocksPtr, Allocator.Persistent);
        }

        public DotGrid32x32x32Unsafe Rent(GridFillMode fillMode)
        {
            switch (fillMode)
            {
                case GridFillMode.Blank:
                    {
                        var p = this.stocksPtr->PopFromFront();
                        if (p != default) return new DotGrid32x32x32Unsafe(p, 0);

                        var p_ = this.stocksPtr->PopFromBack();
                        if (p_ != default) return DotGridAllocater.Fill(p_, fillMode);

                        return DotGridAllocater.Alloc(fillMode);
                    }
                case GridFillMode.Solid:
                    {
                        var p = this.stocksPtr->PopFromBack();
                        if (p != default) return new DotGrid32x32x32Unsafe(p, 32 * 32 * 32);

                        var p_ = this.stocksPtr->PopFromFront();
                        if (p_ != default) return DotGridAllocater.Fill(p_, fillMode);

                        return DotGridAllocater.Alloc(fillMode);
                    }
                default:
                    return new DotGrid32x32x32Unsafe();
            }
        }

        public unsafe void Back(DotGrid32x32x32Unsafe grid, GridFillMode fillMode)
        {
            var isBackSuccess = fillMode switch
            {
                GridFillMode.Blank => this.stocksPtr->PushToFront((UIntPtr)grid.pUnits),
                GridFillMode.Solid => this.stocksPtr->PushToBack((UIntPtr)grid.pUnits),
                _ => true
            };
            if (!isBackSuccess) DotGridAllocater.Dispose((UIntPtr)grid.pUnits);
        }
        public void Back(DotGrid32x32x32Unsafe grid) => Back(grid, grid.FillModeBlankOrSolid);
    }

    public unsafe struct DoubleSideStack<T> : IDisposable
        where T : unmanaged
    {
        //NativeArray<T> buffer;
        [NativeDisableUnsafePtrRestriction]
        T* buffer;
        int bufferLength;
        public int FrontCount { get; private set; }
        public int BackCount { get; private set; }

        public DoubleSideStack(int maxLength)
        {
            //this.buffer = new NativeArray<T>(maxLength, Allocator.Persistent);
            this.buffer = (T*)UnsafeUtility.Malloc(sizeof(T) * maxLength, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            this.bufferLength = maxLength;
            this.FrontCount = 0;
            this.BackCount = 0;
        }
        public void Dispose() => UnsafeUtility.Free(this.buffer, Allocator.Persistent);//this.buffer.Dispose();


        public bool PushToFront(T item)
        {
            if (this.FrontCount + this.BackCount < this.bufferLength)
            {
                var i = this.FrontCount++;
                this.buffer[i] = item;
                return true;
            }
            return false;
        }
        public bool PushToBack(T item)
        {
            if (this.FrontCount + this.BackCount < this.bufferLength)
            {
                var i = this.bufferLength - ++this.BackCount;
                this.buffer[i] = item;
                return true;
            }
            return false;
        }

        public bool PopFromFront(out T item)
        {
            if (this.FrontCount > 0)
            {
                var i = --this.FrontCount;
                item = this.buffer[i];
                return true;
            }
            item = default;
            return false;
        }
        public bool PopFromBack(out T item)
        {
            if (this.BackCount > 0)
            {
                var i = this.bufferLength - this.BackCount--;
                item = this.buffer[i];
                return true;
            }
            item = default;
            return false;
        }

        public T PopFromFront()
        {
            this.PopFromFront(out var item);
            return item;
        }
        public T PopFromBack()
        {
            this.PopFromBack(out var item);
            return item;
        }
    }



}