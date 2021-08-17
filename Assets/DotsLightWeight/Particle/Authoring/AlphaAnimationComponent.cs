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
    public class AlphaAnimationComponent :
        ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public float BlendAlphaTimeSpanSec;
        public float BlendAlphaLast;
        public float DelayBlend;

        public float AdditiveAlphaTimeSpanSec;
        public float AdditiveAlphaLast;
        public float DelayAdditive;

        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;

            var blendAlphaFirst = this.GetComponent<ParticleAuthoring>().BlendColor.a;
            gcs.AddBlendAlphaFadeComponents(this.gameObject, blendAlphaFirst, this.BlendAlphaLast, this.BlendAlphaTimeSpanSec, this.DelayBlend);

            var additiveAlphaFirst = this.GetComponent<ParticleAuthoring>().AddColor.a;
            gcs.AddAdditiveAlphaFadeComponents(this.gameObject, additiveAlphaFirst, this.AdditiveAlphaLast, this.AdditiveAlphaTimeSpanSec, this.DelayAdditive);
        }
    }
}