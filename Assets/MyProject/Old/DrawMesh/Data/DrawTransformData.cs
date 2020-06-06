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


    public struct DrawTransformLinkData : IComponentData
    {
        public Entity DrawModelEntity;
        public Entity DrawInstanceEntity;
    }

    public struct DrawTransformIndexData : IComponentData
    {
        public int BoneId;
        public int BoneLength;
    }

    public unsafe struct DrawTransformTargetWorkData : IComponentData
    {
        public int DrawInstanceId;
    }


}
