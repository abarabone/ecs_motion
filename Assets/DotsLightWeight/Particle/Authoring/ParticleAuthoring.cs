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

    //public interface IParticle
    //{ }
    public abstract class ParticleAuthoringBase : ModelGroupAuthoring.ModelAuthoringBase
    { }


    /// <summary>
    /// 他メッシュとのアトラス対応は後回し
    /// </summary>
    public class ParticleAuthoring :
        ParticleAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
        //ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs,
        //IParticle
    {

        public ParticleModelSourceAuthoring ModelSource;
        //public ParticleMeshType ParticleType;

        public Color ParticleColor;
        public bool UseAnimationAlpha;
        [HideBy("UseAnimationAlpha")] public float AlphaTimeSpanSec;
        [HideBy("UseAnimationAlpha")] public float AlphaLast;

        public float StartRadius;
        public bool UseAnimationRadius;
        public float EndRadius;
        public float EndTimeForRadius;

        public BinaryLength2 CellUsage;

        public int AnimationBaseIndex;
        public bool UseAnimationUv;
        public int AnimationIndexMax;
        public binary_length AnimationIndexLength;
        public float AnimationTimeSpanSec;

        public bool UseLifeTime;
        public float LifeTimeSec;// 0 以下なら消えない

        public bool UseAnimationRotation;
        public float RotationDegreesPerSec;

        public bool UseMoveEasing;
        public float EasingRatePerSec;
        public bool UseRandomDirection;
        public bool UseMinMax;
        public float3 MinEasingSpeed;
        public float3 MaxEasingSpeed;


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

            var modelEntity = gcs.GetPrimaryEntity(this.ModelSource);


            switch (this.ModelSource.ParticleType)
            {
                case ParticleMeshType.billboadUv:
                    gcs.InitParticleUvEntityComponents(this.gameObject, modelEntity, this.ModelSource.Division, this.CellUsage, this.AnimationBaseIndex, this.ParticleColor, this.StartRadius);
                    addParticleEntityComponents_(gcs, this.gameObject);
                    gcs.AddAnimationComponentsOrNot(this.gameObject, this.AnimationIndexLength, this.AnimationBaseIndex, this.AnimationIndexMax, this.AnimationTimeSpanSec);
                    break;

                case ParticleMeshType.psyllium:
                    gcs.InitParticleEntityComponents(this.gameObject, modelEntity, this.ParticleColor, this.StartRadius);
                    addPsylliumComponents_(gcs, this.gameObject);
                    break;

                case ParticleMeshType.psylliumUv:
                    gcs.InitParticleUvEntityComponents(this.gameObject, modelEntity, this.ModelSource.Division, this.CellUsage, this.AnimationBaseIndex, this.ParticleColor, this.StartRadius);
                    addPsylliumComponents_(gcs, this.gameObject);
                    gcs.AddAnimationComponentsOrNot(this.gameObject, this.AnimationIndexLength, this.AnimationBaseIndex, this.AnimationIndexMax, this.AnimationTimeSpanSec);
                    break;
            }

            gcs.AddLifeTimeComponentsOrNot(this.gameObject, this.LifeTimeSec);
            gcs.AddSizingComponentsOrNot(this.gameObject, this.StartRadius, this.EndRadius, this.EndTimeForRadius);
            gcs.AddAlphaFadeComponentsOrNot(this.gameObject, this.ParticleColor.a, this.AlphaLast, this.AlphaTimeSpanSec);
            gcs.AddRotationComponentsOrNot(this.gameObject, this.RotationDegreesPerSec);

            return;


            static void addParticleEntityComponents_(
                GameObjectConversionSystem gcs, GameObject main)
            {
                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);


                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(BillBoad.RotationData),
                    typeof(Translation)
                });
                em.AddComponents(mainEntity, types);

                em.SetComponentData(mainEntity,
                    new BillBoad.RotationData
                    {
                        Direction = new float2(0, 1),
                    }
                );

                em.SetComponentData(mainEntity, new Translation
                {
                    Value = float3.zero,
                });

                em.RemoveComponent<Rotation>(mainEntity);//
            }

            void addPsylliumComponents_(GameObjectConversionSystem gcs, GameObject main)
            {
                dstManager.SetName_(entity, $"{this.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var types = new ComponentTypes(new ComponentType[]
                {
                    //typeof( Particle.TranslationPtoPData ),
                    typeof(Particle.TranslationTailData),
                    typeof(Translation),
                    //typeof(Rotation)
                });
                em.AddComponents(mainEntity, types);

                //em.SetComponentData(mainEntity, new Translation
                //{
                //    Value = float3.zero,
                //});
                //em.SetComponentData(mainEntity, new Rotation
                //{
                //    Value = quaternion.identity,
                //});
                //em.RemoveComponent<Translation>(mainEntity);//
                em.RemoveComponent<Rotation>(mainEntity);//
            }
        }

    }



}
