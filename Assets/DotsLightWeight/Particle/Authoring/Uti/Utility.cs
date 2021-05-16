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

    public enum binary_length_define
    {
        length_1 = 1,
        length_2 = 2,
        length_4 = 4,
        length_8 = 8,
    }


    public static class ParticleAuthoringUtility
    {

        public static int2 AsUint2(this (binary_length_define x, binary_length_define y) x) =>
            new int2((int)x.x, (int)x.y);


        public static void InitParticleEntityComponents(
            this GameObjectConversionSystem gcs, GameObject main, Entity modelEntity,
            int2 division, int2 cellUsage, int animationBaseIndex, Color32 color, float radius)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            gcs.DstEntityManager.SetName_(mainEntity, $"{main.name}");


            var archetype = em.CreateArchetype(
                typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                typeof(DrawInstance.ParticleTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),
                typeof(BillBoad.UvCursorData),
                typeof(BillBoad.CursorToUvIndexData),
                typeof(Particle.AdditionalData)
            );
            em.SetArchetype(mainEntity, archetype);


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
                    UCellUsage = (byte)cellUsage.x,
                    VCellUsage = (byte)cellUsage.y,
                    UMask = (byte)(division.x - 1),
                    VShift = (byte)math.countbits((int)division.x - 1),
                }
            );
            em.SetComponentData(mainEntity,
                new BillBoad.RotationData
                {
                    Direction = new float2(0, 1),
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
            binary_length_define animationIndexLength, float animationTimeSpan)
        {
            if (animationIndexLength <= binary_length_define.length_1) return;

            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(new ComponentType[]
            {
                    typeof(BillBoad.UvAnimationInitializeTag),
                    typeof(BillBoad.UvAnimationWorkData),
                    typeof(BillBoad.UvAnimationData),
            });
            em.AddComponents(mainEntity, types);

            em.SetComponentData(mainEntity,
                new BillBoad.UvAnimationData
                {
                    TimeSpan = animationTimeSpan,
                    TimeSpanR = 1.0f / animationTimeSpan,
                    CursorAnimationMask = (byte)(animationIndexLength - 1),
                }
            );
        }

        public static void AddLifeTimeComponents(this GameObjectConversionSystem gcs, GameObject main, float time)
        {
            if (time <= 0.0f) return;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(Particle.LifeTimeSpecData),
                typeof(Particle.LifeTimeData)
            );
            gcs.DstEntityManager.AddComponents(mainEntity, types);

            gcs.DstEntityManager.AddComponentData(mainEntity, new Particle.LifeTimeSpecData
            {
                DurationSec = time,
            });
        }

    }
}