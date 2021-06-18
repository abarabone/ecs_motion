using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Particle.Aurthoring.disable
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;

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

        [Space(4)]
        [Header("Spring")]
        public bool UseSpring;
        [VisBy("UseSpring")] public float Spring;
        [VisBy("UseSpring")] public float Dumper;
        [VisBy("UseSpring")] public float RestDistance;


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


            gcs.AddParticleComponents(this.gameObject, this.ModelSource, this.ParticleColor, this.StartRadius);

            switch (this.ModelSource.ParticleType)
            {
                case ParticleMeshType.billboadUv:
                    gcs.AddBillBoadComponents(this.gameObject);
                    gcs.AddUvIndexComponents(this.gameObject, this.ModelSource.Division, this.CellUsage, this.AnimationBaseIndex);
                    if (this.UseAnimationUv) gcs.AddUvAnimationComponents(this.gameObject, this.AnimationIndexLength, this.AnimationBaseIndex, this.AnimationIndexMax, this.AnimationTimeSpanSec);
                    break;

                case ParticleMeshType.psyllium:
                    gcs.AddPsylliumComponents(this.gameObject);
                    break;

                case ParticleMeshType.psylliumUv:
                    gcs.AddPsylliumComponents(this.gameObject);
                    gcs.AddUvIndexComponents(this.gameObject, this.ModelSource.Division, this.CellUsage, this.AnimationBaseIndex);
                    if (this.UseAnimationUv) gcs.AddUvAnimationComponents(this.gameObject, this.AnimationIndexLength, this.AnimationBaseIndex, this.AnimationIndexMax, this.AnimationTimeSpanSec);
                    break;

                case ParticleMeshType.LinePsyllium:
                    gcs.AddLineParticleComponents(this.gameObject, this.ModelSource.LineParticleSegments, this.UseSpring);
                    if (this.UseSpring) gcs.AddSpringComponents(this.gameObject, this.Spring, this.Dumper, this.RestDistance, this.ModelSource.LineParticleSegments);
                    break;

                case ParticleMeshType.LineBillboad:

                    break;

                default:
                    break;
            }

            if (this.UseLifeTime) gcs.AddLifeTimeComponents(this.gameObject, this.LifeTimeSec);
            if (this.UseAnimationRadius) gcs.AddSizingComponents(this.gameObject, this.StartRadius, this.EndRadius, this.EndTimeForRadius);
            if (this.UseAnimationAlpha) gcs.AddAlphaFadeComponents(this.gameObject, this.ParticleColor.a, this.AlphaLast, this.AlphaTimeSpanSec);
            if (this.UseAnimationRotation) gcs.AddRotationComponents(this.gameObject, this.MinRotationDegreesPerSec, this.MaxRotationDegreesPerSec);
            if (this.UseMoveEasing) gcs.AddEasingComponents(this.gameObject, this.EasingRatePerSec, this.MinDistanceOffset, this.MaxDistanceOffset, this.UseDirectionNormal, this.DirectionOffset);

            if (!this.UseSpring) gcs.AddMoveTagComponents(this.gameObject);

            return;

        }

    }



}
