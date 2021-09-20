//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Rendering;
//using System.Runtime.InteropServices;
//using System.IO;
//using System.Linq;
//using Unity.Collections;
//using Unity.Mathematics;
//using Unity.Jobs;
//using Unity.Burst;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.MarchingCubes
//{
//    public struct IndirectArgumentsForDispatch
//    {
//        public int x, y, z;

//        public IndirectArgumentsForDispatch(int numx, int numy, int numz)
//        {
//            this.x = numx;
//            this.y = numy;
//            this.z = numz;
//        }

//        public NativeArray<int> ToNativeArray(Allocator allocator)
//        {
//            var arr = new NativeArray<int>(3, allocator);
//            arr[0] = this.x;
//            arr[1] = this.y;
//            arr[2] = this.z;
//            return arr;
//        }
//    }


//    public struct IndirectArgumentsForInstancing
//    {
//        public uint MeshIndexCount;
//        public uint InstanceCount;
//        public uint MeshBaseIndex;
//        public uint MeshBaseVertex;
//        public uint BaseInstance;

//        public IndirectArgumentsForInstancing
//            (Mesh mesh, int instanceCount = 0, int submeshId = 0, int baseInstance = 0)
//        {
//            //if( mesh == null ) return;

//            this.MeshIndexCount = mesh.GetIndexCount(submeshId);
//            this.InstanceCount = (uint)instanceCount;
//            this.MeshBaseIndex = mesh.GetIndexStart(submeshId);
//            this.MeshBaseVertex = mesh.GetBaseVertex(submeshId);
//            this.BaseInstance = (uint)baseInstance;
//        }

//        public NativeArray<uint> ToNativeArray(Allocator allocator)
//        {
//            var arr = new NativeArray<uint>(5, allocator);
//            arr[0] = this.MeshIndexCount;
//            arr[1] = this.InstanceCount;
//            arr[2] = this.MeshBaseIndex;
//            arr[3] = this.MeshBaseVertex;
//            arr[4] = this.BaseInstance;
//            return arr;
//        }
//    }

//    static public class IndirectArgumentsExtensions
//    {
//        static public ComputeBuffer SetData(this ComputeBuffer cbuf, ref IndirectArgumentsForInstancing args)
//        {
//            using (var nativebuf = args.ToNativeArray(Allocator.Temp))
//                cbuf.SetData(nativebuf);

//            return cbuf;
//        }

//        static public ComputeBuffer SetData(this ComputeBuffer cbuf, ref IndirectArgumentsForDispatch args)
//        {
//            using (var nativebuf = args.ToNativeArray(Allocator.Temp))
//                cbuf.SetData(nativebuf);

//            return cbuf;
//        }
//    }

//}
