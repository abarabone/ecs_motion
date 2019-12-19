using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abss.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abss.Draw
{

    public struct DrawTotalData : IComponentData
    {
        
    }

    public struct DrawInstanceData : IComponentData
    {
        public int VectorLengthInBone;
        public int BoneLength;
        //public 
    }



    public struct ThreadSafeCounter
    {

    }
}
