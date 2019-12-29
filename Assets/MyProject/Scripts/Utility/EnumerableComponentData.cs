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
using Unity.Collections.LowLevel.Unsafe;


namespace Abss.Misc
{

    public struct EnumerableComponentData<T> : System.IDisposable
        where T : unmanaged, IComponentData
    {

        ArchetypeChunkComponentType<T> ctype;
        NativeArray<ArchetypeChunk> chunks;


        public EnumerableComponentData( JobComponentSystem sys, EntityQuery query, bool isReadOnly )
        {
            this.ctype = sys.GetArchetypeChunkComponentType<T>( isReadOnly );
            this.chunks = query.CreateArchetypeChunkArray( Allocator.Temp );
        }

        public void Dispose()
        {
            this.chunks.Dispose();
        }

        public ComponentDataEnumerator GetEnumerator()
        {
            return new ComponentDataEnumerator( this.chunks, this.ctype );
        }



        public struct ComponentDataEnumerator
        {
            ArchetypeChunkComponentType<T> ctype;

            NativeArray<ArchetypeChunk> chunks;
            NativeArray<ArchetypeChunk>.Enumerator chunkEnumerator;

            NativeArray<T> currentArray;
            int arrayIndex;


            public ComponentDataEnumerator
                ( NativeArray<ArchetypeChunk> chunks, ArchetypeChunkComponentType<T> ctype )
            {
                this.ctype = ctype;
                this.chunks = chunks;
                this.chunkEnumerator = this.chunks.GetEnumerator();
                this.currentArray = new NativeArray<T>();
                this.arrayIndex = 0;
            }


            public unsafe ref T Current =>
                ref ( (T*)NativeArrayUnsafeUtility.GetUnsafePtr( this.currentArray ) )[ this.arrayIndex ];


            public bool MoveNext()
            {
                if( ++this.arrayIndex >= this.currentArray.Length )
                {
                    if( !this.chunkEnumerator.MoveNext() ) return false;

                    this.currentArray = this.chunkEnumerator.Current.GetNativeArray( this.ctype );
                    this.arrayIndex = 0;
                }

                return true;
            }

            //public void Reset()
            //{
            //    throw new System.NotImplementedException();
            //}
        }

    }
}
