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
    /// 
    /// </summary>
    public class PsylliumAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public ParticleModelSourceAuthoring ModelSource;

        public Color ParticleColor;
        public float Radius;

        public uint2 CellUsage;

        public int AnimationBaseIndex;
        public binary_length_define AnimationIndexLength;
        public float AnimationTimeSpan;


        public float LifeTimeSec;// 0 以下なら消えない


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.ModelSource.gameObject);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var modelEntity = conversionSystem.GetPrimaryEntity(this.ModelSource);



            initParticleEntityComponents_(conversionSystem, this.gameObject, modelEntity, this);

            conversionSystem.AddAnimationComponents(this.gameObject, this.AnimationIndexLength, this.AnimationTimeSpan);

            conversionSystem.AddLifeTimeComponents(this.gameObject, this.LifeTimeSec);

            return;



            void initParticleEntityComponents_( GameObjectConversionSystem gcs, Entity modelEntity, GameObject main )
            {
                dstManager.SetName_( entity, $"{this.name}" );

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity( main );

                var archetype = em.CreateArchetype(
                    typeof( ModelPrefabNoNeedLinkedEntityGroupTag ),
                    typeof( DrawInstance.ParticleTag ),
                    typeof( DrawInstance.ModelLinkData ),
                    typeof( DrawInstance.TargetWorkData ),
                    typeof( Particle.AdditionalData ),
                    typeof( Particle.TranslationPtoPData ),
                    typeof(Translation),
                    typeof(Rotation)
                );
                em.SetArchetype( mainEntity, archetype );


                em.SetComponentData( mainEntity,
                    new DrawInstance.ModelLinkData
                    {
                        DrawModelEntityCurrent = modelEntity,
                    }
                );
                em.SetComponentData( mainEntity,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );
                em.SetComponentData( mainEntity,
                    new Particle.AdditionalData
                    {
                        Color = this.Material.color,
                        Size = this.DefaultRadius,
                    }
                );

            }

        }
    }
    
}
