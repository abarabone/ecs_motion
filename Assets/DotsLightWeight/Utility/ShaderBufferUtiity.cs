using UnityEngine;
using Unity.Collections;
using System;
using System.Runtime.CompilerServices;

namespace DotsLite.Draw
{

    static public class ComputeShaderUtility
    {




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ComputeBuffer CreateIndirectArgumentsBuffer() =>
            new ComputeBuffer(1, sizeof(uint) * 5, ComputeBufferType.IndirectArguments, ComputeBufferMode.Immutable);

    }


    public struct IndirectArgumentsForInstancing
    {
        public uint MeshIndexCount;
        public uint InstanceCount;
        public uint MeshBaseIndex;
        public uint MeshBaseVertex;
        public uint BaseInstance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndirectArgumentsForInstancing(
            Mesh mesh, int instanceCount = 0, int submeshId = 0, int baseInstance = 0)
        {
            //if( mesh == null ) return;

            this.MeshIndexCount = mesh.GetIndexCount(submeshId);
            this.InstanceCount = (uint)instanceCount;
            this.MeshBaseIndex = mesh.GetIndexStart(submeshId);
            this.MeshBaseVertex = mesh.GetBaseVertex(submeshId);
            this.BaseInstance = (uint)baseInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<uint> ToNativeArray(Allocator allocator)
        {
            var arr = new NativeArray<uint>(5, allocator);
            arr[0] = this.MeshIndexCount;
            arr[1] = this.InstanceCount;
            arr[2] = this.MeshBaseIndex;
            arr[3] = this.MeshBaseVertex;
            arr[4] = this.BaseInstance;
            return arr;
        }
    }
    //public struct IndirectArgumentsForInstancing_ : IDisposable
    //{
    //    public uint MeshIndexCount { get => this.entries[0]; set => this.entries[0] = value; }
    //    public uint InstanceCount { get => this.entries[1]; set => this.entries[1] = value; }
    //    public uint MeshBaseIndex { get => this.entries[2]; set => this.entries[2] = value; }
    //    public uint MeshBaseVertex { get => this.entries[3]; set => this.entries[3] = value; }
    //    public uint BaseInstance { get => this.entries[4]; set => this.entries[4] = value; }

    //    public NativeArray<uint> AsNativeArray() => this.entries;


    //    NativeArray<uint> entries;
        
    //    public void Dispose() => this.entries.Dispose();


    //    public IndirectArgumentsForInstancing_(
    //        Mesh mesh, int instanceCount = 0, int submeshId = 0, int baseInstance = 0)
    //    {
    //        this.entries = new NativeArray<uint>(5, Allocator.Temp);

    //        this.MeshIndexCount = mesh.GetIndexCount(submeshId);
    //        this.InstanceCount = (uint)instanceCount;
    //        this.MeshBaseIndex = mesh.GetIndexStart(submeshId);
    //        this.MeshBaseVertex = mesh.GetBaseVertex(submeshId);
    //        this.BaseInstance = (uint)baseInstance;
    //    }
    //}

    static public class IndirectArgumentsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public ComputeBuffer SetData(this ComputeBuffer cbuf, ref IndirectArgumentsForInstancing args)
        {
            using (var nativebuf = args.ToNativeArray(Allocator.Temp))
                cbuf.SetData(nativebuf);

            return cbuf;
        }
        //static public void SetData(this ComputeBuffer cbuf, IndirectArgumentsForInstancing_ args) =>
        //    cbuf.SetData(args.AsNativeArray());
    }


}