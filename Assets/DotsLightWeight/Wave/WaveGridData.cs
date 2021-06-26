using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.WaveGrid
{
    public class WaveGridMasterData : IComponentData, IDisposable
    {
        public NativeArray<WaveGridPoint> Units;
        public int2 UnitLengthInGrid;
        public int2 NumGrids;

        public void Dispose()
        {
            this.Units.Dispose();
        }
    }
    public struct WaveGridPoint
    {
        public float Next;
        public float Curr;
        public float Prev;
    }

    public struct WaveGridData : IComponentData
    {
        public int2 gridid;
        public float UnitScaleOnLod;
    }
}