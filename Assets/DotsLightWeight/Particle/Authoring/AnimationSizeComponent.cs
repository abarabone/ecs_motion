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

    [RequireComponent(typeof(ParticleAuthoring))]
    public class AnimationSizeComponent :
        ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public float EndRadius;
        public float EndTimeSpanSec;

        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }

            var gcs = conversionSystem;

            var radiusFirst = this.GetComponent<ParticleAuthoring>().Radius;
            gcs.AddSizingComponents(this.gameObject, radiusFirst, this.EndRadius, this.EndTimeSpanSec);
        }
    }
}