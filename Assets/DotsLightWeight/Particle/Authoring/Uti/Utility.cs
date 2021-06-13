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
        LinePsyllium,
        LineBillboad,
    }

    public static class ParticleAuthoringUtility
    {

        //public static int2 AsUint2(this (binary_length x, binary_length y) x) =>
        //    new int2((int)x.x, (int)x.y);


        public static void AddParticleComponents(
            this GameObjectConversionSystem gcs, GameObject main, ParticleModelSourceAuthoring modelSource, Color32 color, float radius)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            gcs.DstEntityManager.SetName_(mainEntity, $"{main.name}");


            var types = new ComponentTypes(new ComponentType[]
            {
                typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),
                typeof(Particle.AdditionalData),
            });
            em.AddComponents(mainEntity, types);


            em.SetComponentData(mainEntity,
                new DrawInstance.ModelLinkData
                {
                    DrawModelEntityCurrent = gcs.GetPrimaryEntity(modelSource),
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
                    Radius = radius,
                }
            );
        }

        public static void AddBillBoadComponents(
            this GameObjectConversionSystem gcs, GameObject main)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);


            var types = new ComponentTypes(new ComponentType[]
            {
                    typeof(DrawInstance.BillBoadTag),
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

        public static void AddPsylliumComponents(
            this GameObjectConversionSystem gcs, GameObject main)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(DrawInstance.PsylliumTag),
                typeof(Translation),
                typeof(Psyllium.TranslationTailData)
            );
            em.AddComponents(mainEntity, types);

            em.RemoveComponent<Rotation>(mainEntity);//
        }

        public static void AddLineParticleComponents(
            this GameObjectConversionSystem gcs, GameObject main, int segments)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(DrawInstance.LineParticleTag),
                typeof(Translation),
                typeof(Psyllium.TranslationTailData),
                typeof(LineParticle.TranslationTailLineData)
            );
            em.AddComponents(mainEntity, types);

            var buffer = em.AddBuffer<LineParticle.TranslationTailLineData>(mainEntity);
            buffer.Length = segments + 1 - 2;

            em.RemoveComponent<Rotation>(mainEntity);//
        }


        public static void AddUvIndexComponents(
            this GameObjectConversionSystem gcs, GameObject main,
            BinaryLength2 division, BinaryLength2 cellUsage, int uvIndex)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(new ComponentType[]
            {
                typeof(BillBoad.UvCursorData),
                typeof(BillBoad.CursorToUvIndexData),
            });
            em.AddComponents(mainEntity, types);


            em.SetComponentData(mainEntity,
                new BillBoad.UvCursorData
                {
                    CurrentIndex = 0,
                }
            );
            em.SetComponentData(mainEntity,
                new BillBoad.CursorToUvIndexData
                {
                    IndexOffset = uvIndex,
                    UCellUsage = (byte)cellUsage.u,
                    VCellUsage = (byte)cellUsage.v,
                    UMask = (byte)(division.u - 1),
                    VShift = (byte)math.countbits((int)division.u - 1),
                }
            );
        }

        public static void AddUvAnimationComponents(
            this GameObjectConversionSystem gcs, GameObject main,
            binary_length animationIndexLength, int animationBaseIndex, int animationIndexMax, float animationTimeSpan)
        {
            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(new ComponentType[]
            {
                typeof(Particle.LifeTimeInitializeTag),
                typeof(Particle.LifeTimeData),
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


        public static void AddRotationComponents(
            this GameObjectConversionSystem gcs, GameObject main, float rotSpeedMin, float rotSpeedMax)
        {

            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var _types = new List<ComponentType>
            {
                typeof(Particle.LifeTimeInitializeTag),
                typeof(BillBoad.RotationSpeedData),
            };
            if (rotSpeedMin != rotSpeedMax)
            {
                _types.Add(typeof(BillBoad.RotationRandomSettingData));
            }
            var types = new ComponentTypes(_types.ToArray());
            em.AddComponents(mainEntity, types);

            em.AddComponentData(mainEntity,
                new BillBoad.RotationSpeedData
                {
                    RadSpeedPerSec = math.radians(rotSpeedMin),
                }
            );
            if (rotSpeedMin != rotSpeedMax)
            {
                em.AddComponentData(mainEntity,
                    new BillBoad.RotationRandomSettingData
                    {
                        MinSpeed = rotSpeedMin,
                        MaxSpeed = rotSpeedMax,
                    }
                );
            }
        }


        public static void AddAlphaFadeComponents(
            this GameObjectConversionSystem gcs, GameObject main, float firstValue, float lastValue, float timeSpan)
        {
            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(BillBoad.AlphaFadeData)
            );
            em.AddComponents(mainEntity, types);

            em.AddComponentData(mainEntity,
                new BillBoad.AlphaFadeData
                {
                    Current = firstValue,
                    Min = math.min(firstValue, lastValue),
                    Max = math.max(firstValue, lastValue),
                    SpeedPerSec = (lastValue - firstValue) / timeSpan,
                }
            );
        }


        public static void AddSizingComponents(
            this GameObjectConversionSystem gcs, GameObject main,
            float startRadius, float endRadius, float endtime)
        {
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


        public static void AddEasingComponents(
            this GameObjectConversionSystem gcs, GameObject main,
            float rate, float distOffsetMin, float distOffsetMax,
            bool useDirectionSetting, float3 dir)
        {

            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);
            

            var _types = new List<ComponentType>
            {
                typeof(Particle.LifeTimeInitializeTag),
                typeof(Particle.EasingData)
            };
            if (!useDirectionSetting)
            {
                _types.Add(typeof(Particle.EasingSetting));
            }
            var types = new ComponentTypes(_types.ToArray());
            em.AddComponents(mainEntity, types);


            em.AddComponentData(mainEntity,
                new Particle.EasingData
                {
                    LastPosition = useDirectionSetting ? dir : 0,
                    Rate = rate,
                }
            );
            em.AddComponentData(mainEntity,
                new Particle.EasingSetting
                {
                    LastDistanceMin = distOffsetMin,
                    LastDistanceMax = distOffsetMax,
                }
            );
        }


        public static void AddSpringComponents(
            this GameObjectConversionSystem gcs, GameObject main,
            float spring, float dumper, float rest, int segments)
        {
            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(Spring.SpecData),
                typeof(Spring.StateData)
            );
            em.AddComponents(mainEntity, types);

            em.AddComponentData(mainEntity,
                new Spring.SpecData
                {
                    Spring = spring,
                    Dumper = dumper,
                    Rest = rest,
                }
            );

            var buffer = em.AddBuffer<Spring.StateData>(mainEntity);
            buffer.Length = segments + 1;
        }

    }
}