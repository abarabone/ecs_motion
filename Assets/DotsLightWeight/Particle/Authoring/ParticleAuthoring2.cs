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

    /// <summary>
    /// 他メッシュとのアトラス対応は後回し
    /// </summary>
    public class ParticleAuthoring2 :
        ParticleAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public ParticleModelSourceAuthoring ModelSource;


        [Header("Base Option")]
        public Color ParticleColor;
        public float Radius;

        [Header("Texture Cell")]
        public int CellIndex;
        public BinaryLength2 CellUsage;



        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.ModelSource.gameObject);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;


            gcs.AddParticleComponents(this.gameObject, this.ModelSource, this.ParticleColor, this.Radius);

            switch (this.ModelSource.ParticleType)
            {
                case ParticleMeshType.billboadUv:
                    gcs.AddBillBoadComponents(this.gameObject);
                    gcs.AddUvIndexComponents(this.gameObject, this.ModelSource.Division, this.CellUsage, this.CellIndex);
                    break;

                case ParticleMeshType.psyllium:
                    gcs.AddPsylliumComponents(this.gameObject);
                    break;

                case ParticleMeshType.psylliumUv:
                    gcs.AddPsylliumComponents(this.gameObject);
                    gcs.AddUvIndexComponents(this.gameObject, this.ModelSource.Division, this.CellUsage, this.CellIndex);
                    break;

                case ParticleMeshType.LinePsyllium:
                    var useSpring = this.GetComponent<SpringComponent>() != null;
                    gcs.AddLineParticleComponents(this.gameObject, this.ModelSource.LineParticleSegments, useSpring);
                    break;

                case ParticleMeshType.LineBillboad:

                    break;

                default:
                    break;
            }
        }

    }



}
