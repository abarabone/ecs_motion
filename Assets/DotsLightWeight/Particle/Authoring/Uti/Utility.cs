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

    [Serializable]
    public struct BinaryLength2
    {
        public binary_length u;
        public binary_length v;
        public static implicit operator int2(BinaryLength2 src) => new int2((int)src.u, (int)src.v);
    }
    public enum binary_length
    {
        length_1 = 1,
        length_2 = 2,
        length_4 = 4,
        length_8 = 8,
        length_16 = 16,
        //length_32 = 32,
        //length_64 = 64,
        //length_128 = 128,
        //length_256 = 256,
    }

    public enum ParticleMeshType
    {
        billboadUv,
        psyllium,
        psylliumUv,
    }

    public static class ParticleAuthoringUtility
    {

        //public static int2 AsUint2(this (binary_length x, binary_length y) x) =>
        //    new int2((int)x.x, (int)x.y);


        public static void InitParticleUvEntityComponents(
            this GameObjectConversionSystem gcs, GameObject main, Entity modelEntity,
            BinaryLength2 division, BinaryLength2 cellUsage, int animationBaseIndex, Color32 color, float radius)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            gcs.DstEntityManager.SetName_(mainEntity, $"{main.name}");


            var types = new ComponentTypes(new ComponentType[]
            {
                typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                typeof(DrawInstance.ParticleTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),
                typeof(BillBoad.UvCursorData),
                typeof(BillBoad.CursorToUvIndexData),
                typeof(Particle.AdditionalData),
            });
            em.AddComponents(mainEntity, types);


            em.SetComponentData(mainEntity,
                new DrawInstance.ModelLinkData
                {
                    DrawModelEntityCurrent = modelEntity,
                }
            );
            em.SetComponentData(mainEntity,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );

            em.SetComponentData(mainEntity,
                new BillBoad.UvCursorData
                {
                    CurrentIndex = 0,
                }
            );
            em.SetComponentData(mainEntity,
                new BillBoad.CursorToUvIndexData
                {
                    IndexOffset = animationBaseIndex,
                    UCellUsage = (byte)cellUsage.u,
                    VCellUsage = (byte)cellUsage.v,
                    UMask = (byte)(division.u - 1),
                    VShift = (byte)math.countbits((int)division.u - 1),
                }
            );

            em.SetComponentData(mainEntity,
                new Particle.AdditionalData
                {
                    Color = color,
                    Size = radius,
                }
            );
        }


        public static void InitParticleEntityComponents(
            this GameObjectConversionSystem gcs, GameObject main, Entity modelEntity, Color32 color, float radius)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            gcs.DstEntityManager.SetName_(mainEntity, $"{main.name}");


            var types = new ComponentTypes(new ComponentType[]
            {
                typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                typeof(DrawInstance.ParticleTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),
                typeof(Particle.AdditionalData),
            });
            em.AddComponents(mainEntity, types);


            em.SetComponentData(mainEntity,
                new DrawInstance.ModelLinkData
                {
                    DrawModelEntityCurrent = modelEntity,
                }
            );
            em.SetComponentData(mainEntity,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );

            em.SetComponentData(mainEntity,
                new Particle.AdditionalData
                {
                    Color = color,
                    Size = radius,
                }
            );
        }


        public static void AddAnimationComponents(
            this GameObjectConversionSystem gcs, GameObject main,
            binary_length animationIndexLength, int animationBaseIndex, int animationIndexMax, float animationTimeSpan)
        {
            if (animationIndexLength <= binary_length.length_1) return;

            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(new ComponentType[]
            {
                typeof(Particle.LifeTimeInitializeTag),
                typeof(Particle.LifeTimeData),
                //typeof(BillBoad.UvAnimationInitializeTag),
                typeof(BillBoad.UvAnimationWorkData),
                typeof(BillBoad.UvAnimationData),
            });
            em.AddComponents(mainEntity, types);

            em.SetComponentData(mainEntity,
                new BillBoad.UvAnimationData
                {
                    TimeSpan = animationTimeSpan,
                    TimeSpanR = 1.0f / animationTimeSpan,
                    CursorAnimationMask = (int)animationIndexLength - 1,
                    AnimationIndexMax = animationIndexMax - animationBaseIndex,
                }
            );
        }

        public static void AddLifeTimeComponents(
            this GameObjectConversionSystem gcs, GameObject main, float time)
        {
            if (time <= 0.0f) return;

            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(Particle.LifeTimeInitializeTag),
                typeof(Particle.LifeTimeSpecData),
                typeof(Particle.LifeTimeData)
            );
            em.AddComponents(mainEntity, types);

            em.AddComponentData(mainEntity,
                new Particle.LifeTimeSpecData
                {
                    DurationSec = time,
                }
            );
        }

        public static void AddSizingComponents(
            this GameObjectConversionSystem gcs, GameObject main,
            float startRadius, float endRadius, float endtime)
        {
            if (endtime <= 0.0f) return;

            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(Particle.LifeTimeInitializeTag),
                typeof(Particle.LifeTimeData),
                typeof(BillBoad.SizeAnimationData)
            );
            em.AddComponents(mainEntity, types);

            em.AddComponentData(mainEntity,
                new BillBoad.SizeAnimationData
                {
                    StartSize = startRadius,
                    EndSize = endRadius,
                    MaxTimeSpanR = 1.0f / endtime,
                }
            );
        }

    }
}