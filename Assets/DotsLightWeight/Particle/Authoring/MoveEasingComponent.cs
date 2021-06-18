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
    public class MoveEasingComponent :
        ParticleAuthoringBase, IConvertGameObjectToEntity
    {

        public float EasingRatePerSec;

        public float MinDistanceOffset;
        public float MaxDistanceOffset;

        //public float3 MoveDirectionRad;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;

            var useDirectionNormal = false;

            gcs.AddEasingComponents(this.gameObject,
                this.EasingRatePerSec, this.MinDistanceOffset, this.MaxDistanceOffset, useDirectionNormal, float3.zero);// this.MoveDirection);
        }
    }
}