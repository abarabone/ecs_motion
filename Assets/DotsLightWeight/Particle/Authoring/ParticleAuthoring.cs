using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.ParticleSystem.Aurthoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Authoring;

    /// <summary>
    /// 他メッシュとのアトラス対応は後回し
    /// </summary>
    public class ParticleAuthoring :
        ParticleAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public ParticleModelSourceAuthoring ModelSource;


        public Color BlendColor = new Color(1, 1, 1, 1);
        public Color AddColor = new Color(0, 0, 0, 0);
        public float Radius;

        [Header("Texture Cell Option")]
        public int CellIndex;
        public BinaryLength2 CellUsage = new BinaryLength2
        {
            u = binary_length.length_1,
            v = binary_length.length_1,
        };



        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }

            referencedPrefabs.Add(this.ModelSource.gameObject);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }

            var gcs = conversionSystem;


            gcs.AddParticleComponents(this.gameObject, this.ModelSource, this.BlendColor, this.AddColor, this.Radius);

            var useSpring = this.GetComponent<SpringComponent>() != null;
            if (!useSpring) gcs.AddMoveTagComponents(this.gameObject);

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
