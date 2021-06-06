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

        [Header("Color")]
        public Color ParticleColor;
        public bool UseAnimationAlpha;
        [VisBy("UseAnimationAlpha")] public float AlphaTimeSpanSec;
        [VisBy("UseAnimationAlpha")] public float AlphaLast;

        [Space(4)]
        [Header("Size")]
        public float StartRadius;
        public bool UseAnimationRadius;
        [VisBy("UseAnimationRadius")] public float EndRadius;
        [VisBy("UseAnimationRadius")] public float EndTimeForRadius;

        [Space(4)]
        [Header("Cell")]//[Compact]
        public BinaryLength2 CellUsage;

        [Space(4)]
        [Header("UvIndex")]
        public int AnimationBaseIndex;
        public bool UseAnimationUv;
        [VisBy("UseAnimationUv")] public int AnimationIndexMax;
        [VisBy("UseAnimationUv")] public binary_length AnimationIndexLength;
        [VisBy("UseAnimationUv")] public float AnimationTimeSpanSec;

        [Space(4)]
        [Header("LifeTime")]
        public bool UseLifeTime;
        [VisBy("UseLifeTime")] public float LifeTimeSec;

        [Space(4)]
        [Header("Rotation")]
        public bool UseAnimationRotation;
        [VisBy("UseAnimationRotation")] public float MinRotationDegreesPerSec;
        [VisBy("UseAnimationRotation")] public float MaxRotationDegreesPerSec;
        //public bool UseRandomRotation;

        [Space(4)]
        [Header("Move")]
        public bool UseMoveEasing;
        [VisBy("UseMoveEasing")] public float EasingRatePerSec;
        public bool UseDirectionNormal;
        [VisBy("UseDirectionOffset")] public float3 DirectionOffset;
        public float MinDistanceOffset;
        public float MaxDistanceOffset;
        //public bool UseRandomMove;


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


            gcs.AddParticleEntityComponents(this.gameObject, modelEntity, this.ParticleColor, this.StartRadius);

            switch (this.ModelSource.ParticleType)
            {
                case ParticleMeshType.billboadUv:
                    gcs.AddBillBoadComponents(this.gameObject);
                    gcs.AddUvIndexComponents(this.gameObject, this.ModelSource.Division, this.CellUsage, this.AnimationBaseIndex);
                    gcs.AddUvAnimationComponentsOrNot(this.gameObject, this.AnimationIndexLength, this.AnimationBaseIndex, this.AnimationIndexMax, this.AnimationTimeSpanSec, this.UseAnimationUv);
                    break;

                case ParticleMeshType.psyllium:
                    gcs.AddPsylliumComponents(this.gameObject);
                    break;

                case ParticleMeshType.psylliumUv:
                    gcs.AddPsylliumComponents(this.gameObject);
                    gcs.AddUvIndexComponents(this.gameObject, this.ModelSource.Division, this.CellUsage, this.AnimationBaseIndex);
                    gcs.AddUvAnimationComponentsOrNot(this.gameObject, this.AnimationIndexLength, this.AnimationBaseIndex, this.AnimationIndexMax, this.AnimationTimeSpanSec, this.UseAnimationUv);
                    break;
            }

            gcs.AddLifeTimeComponentsOrNot(this.gameObject, this.LifeTimeSec, this.UseLifeTime);
            gcs.AddSizingComponentsOrNot(this.gameObject, this.StartRadius, this.EndRadius, this.EndTimeForRadius, this.UseAnimationRadius);
            gcs.AddAlphaFadeComponentsOrNot(this.gameObject, this.ParticleColor.a, this.AlphaLast, this.AlphaTimeSpanSec, this.UseAnimationAlpha);
            gcs.AddRotationComponentsOrNot(this.gameObject, this.MinRotationDegreesPerSec, this.MaxDistanceOffset, this.UseAnimationRotation);
            gcs.AddEasingComponentsOrNot(this.gameObject, this.EasingRatePerSec, this.MinDistanceOffset, this.MaxDistanceOffset, this.UseDirectionNormal, this.DirectionOffset, this.UseMoveEasing);

            return;

        }

    }



}
