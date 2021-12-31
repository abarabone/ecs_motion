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
    using DotsLite.Authoring;

    [RequireComponent(typeof(ParticleAuthoring))]
    public class AnimationUvComponent :
        ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public int CellIndexLast;
        public binary_length CellIndexLength;
        public float AnimationTimeSpanSec;

        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) return;

            var gcs = conversionSystem;

            var baseindex = this.GetComponent<ParticleAuthoring>().CellIndex;
            gcs.AddUvAnimationComponents(this.gameObject,
                this.CellIndexLength, baseindex, this.CellIndexLast, this.AnimationTimeSpanSec);
        }
    }
}