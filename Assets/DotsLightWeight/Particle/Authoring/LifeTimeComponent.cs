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
    public class LifeTimeComponent :
        ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public float LifeTimeSec;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;

            gcs.AddLifeTimeComponents(this.gameObject, this.LifeTimeSec);
        }
    }
}