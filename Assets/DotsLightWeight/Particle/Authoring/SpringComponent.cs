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
    public class SpringComponent :
        ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public float Spring;
        public float Dumper;
        public float RestDistance;



        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;

            var segmentLength = this.GetComponent<ParticleAuthoring>().ModelSource.LineParticleSegments;
            gcs.AddSpringComponents(this.gameObject, this.Spring, this.Dumper, this.RestDistance, segmentLength);
        }
    }
}