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

namespace Abarabone.MarchingCubes
{


    public struct CubeInstance
    {
        public uint instance;
        static public implicit operator CubeInstance(uint cubeInstance) => new CubeInstance { instance = cubeInstance };
    }


}
