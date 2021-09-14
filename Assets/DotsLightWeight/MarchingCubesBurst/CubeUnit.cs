using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System;

namespace DotsLite.MarchingCubes
{


    [StructLayout(LayoutKind.Sequential)]
    public struct CubeInstance
    {
        public uint instance;
        static public implicit operator CubeInstance(uint cubeInstance) => new CubeInstance { instance = cubeInstance };
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct GridInstanceData
    {
        public float4 Position;
        //public ushort back, up, left, current, right, down, forward;
        //private ushort dummy;
        public uint4 ortho;
    }

}
