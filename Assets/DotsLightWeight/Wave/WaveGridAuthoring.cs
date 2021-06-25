using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.WaveGrid.Aurthoring
{
    using DotsLite.WaveGrid;
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;

    public class WaveGridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public float UnitDistance;
        public int2 UnitLength;



        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;

            dstManager.AddComponentData(entity, new WaveGridData
            {
                Units = new NativeArray<WaveUnit>(this.UnitLength.x * this.UnitLength.y, Allocator.Persistent),
            });
        }
    }
}
namespace DotsLite.WaveGrid
{
    public class WaveGridData : IComponentData, IDisposable
    {
        public NativeArray<WaveUnit> Units;

        public void Dispose() => this.Units.Dispose();
    }
    public struct WaveUnit
    {
        public float Next;
        public float Curr;
        public float Prev;
    }
}