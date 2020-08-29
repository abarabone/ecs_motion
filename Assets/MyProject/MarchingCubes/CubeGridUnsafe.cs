using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace MarchingCubes
{


    public struct CubeInstance
    {
        public uint instance;
        static public implicit operator CubeInstance( uint cubeInstance ) => new CubeInstance { instance = cubeInstance };
    }



    public unsafe struct CubeGrid32x32x32Unsafe
    {
        public uint* pUnits { get; private set; }

        public int CubeCount { get; private set; }
        public bool IsFullOrEmpty => ( this.CubeCount & 0x7fff ) == 0;
        public bool IsFull => this.CubeCount == 0x8000;
        public bool IsEmpty => this.CubeCount == 0;

        
        public CubeGrid32x32x32Unsafe( bool isFillAll ) : this()
        {
            //var align = UnsafeUtility.AlignOf<uint4>();
            const int align = 16;
            const int size = sizeof( uint ) * 1 * 32 * 32;

            this.alloc_( size, align, isFillAll );
        }

        static public CubeGrid32x32x32Unsafe GetDefault( bool isFillAll )
        {
            //var align = UnsafeUtility.AlignOf<uint4>();
            const int align = 16;
            //const int size = sizeof( uint ) * 4;
            const int size = sizeof( uint ) * 1 * 32 * 32;

            var grid = new CubeGrid32x32x32Unsafe();
            grid.alloc_( size, align, isFillAll );

            return grid;
        }

        void alloc_( int size, int align, bool isFillAll )
        {
            this.Dispose();
            this.pUnits = (uint*)UnsafeUtility.Malloc( size, align, Allocator.Persistent );

            if( isFillAll )
            {
                UnsafeUtility.MemSet( this.pUnits, 0xff, size );
                this.CubeCount = 32 * 32 * 32;
            }
            else
            {
                UnsafeUtility.MemClear( this.pUnits, size );
                this.CubeCount = 0;
            }
        }

        public void Dispose()
        {
            if( this.pUnits == null ) return;// struct なので、複製された場合はこのチェックも意味がない
            
            UnsafeUtility.Free( this.pUnits, Allocator.Persistent );
            this.pUnits = null;
        }


        public uint this[ int ix, int iy, int iz ]
        {
            get
            {
                return (uint)( this.pUnits[ ( iy << 5 ) + iz ] >> ix & 1 );
            }
            set
            {
                var maskedValue = value & 1;

                var i = ( iy << 5 ) + iz;
                var b = this.pUnits[ i ];
                this.pUnits[ i ] ^= (uint)( (b & 1 << ix) ^ (maskedValue << ix) );

                this.CubeCount += (int)( (maskedValue << 1) - 1 );
            }
        }
    }
}
