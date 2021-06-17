using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Particle.Aurthoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;

    [RequireComponent(typeof(ParticleAuthoring2))]
    public class UvAnimationComponent :
        ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public int CellIndexMax;
        public binary_length CellIndexLength;
        public float AnimationTimeSpanSec;

        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;

            var baseindex = this.GetComponent<ParticleAuthoring2>().CellIndex;
            gcs.AddUvAnimationComponents(this.gameObject,
                this.CellIndexLength, baseindex, this.CellIndexMax, this.AnimationTimeSpanSec);
        }
    }
}