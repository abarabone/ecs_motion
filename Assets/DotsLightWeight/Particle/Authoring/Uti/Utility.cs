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
    using DotsLite.Authoring;

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
            this GameObjectConversionSystem gcs, GameObject main, ParticleModelSourceAuthoring modelSource,
            Color32 blendcolor, Color32 addcolor, float radius)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            gcs.DstEntityManager.SetName_(mainEntity, $"{main.name}");


            var types = new ComponentTypes(new ComponentType[]
            {
                typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),
                typeof(Particle.OptionalData),
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
                new Particle.OptionalData
                {
                    BlendColor = blendcolor,
                    AdditiveColor = addcolor,
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
            this GameObjectConversionSystem gcs, GameObject main, int segments, bool isSpring)
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
            buffer.Length = isSpring ? segments + 1 : segments + 1 - 2;

            em.RemoveComponent<Rotation>(mainEntity);//
        }

        //public static void AddSpringLineParticleComponents(
        //    this GameObjectConversionSystem gcs, GameObject main, int segments)
        //{
        //    var em = gcs.DstEntityManager;


        //    var mainEntity = gcs.GetPrimaryEntity(main);

        //    var types = new ComponentTypes(
        //        typeof(DrawInstance.LineParticleTag),
        //        typeof(Translation),
        //        typeof(Psyllium.TranslationTailData),
        //        typeof(LineParticle.TranslationTailLineData)
        //    );
        //    em.AddComponents(mainEntity, types);

        //    var buffer = em.AddBuffer<LineParticle.TranslationTailLineData>(mainEntity);
        //    buffer.Length = segments + 1;

        //    em.RemoveComponent<Rotation>(mainEntity);//
        //}



        public static void AddMoveTagComponents(
            this GameObjectConversionSystem gcs, GameObject main)
        {
            var em = gcs.DstEntityManager;


            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(Psyllium.MoveTailTag)
            );
            em.AddComponents(mainEntity, types);
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

            em.SetComponentData(mainEntity,
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

            em.SetComponentData(mainEntity,
                new BillBoad.RotationSpeedData
                {
                    RadSpeedPerSec = math.radians(rotSpeedMin),
                }
            );
            if (rotSpeedMin != rotSpeedMax)
            {
                em.SetComponentData(mainEntity,
                    new BillBoad.RotationRandomSettingData
                    {
                        MinSpeed = rotSpeedMin,
                        MaxSpeed = rotSpeedMax,
                    }
                );
            }
        }



        public static void AddAlphaFadeComponent(
            this GameObjectConversionSystem gcs, GameObject main,
            (float firstValue, float lastValue, float timeSpan, float delay) blend,
            (float firstValue, float lastValue, float timeSpan, float delay) add)
        {
            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetPrimaryEntity(main);

            var types = new ComponentTypes(
                typeof(BillBoad.AlphaFadeData)
            );
            em.AddComponents(mainEntity, types);

            var firstValue = new float4(blend.firstValue, add.firstValue, 0, 0);
            var lastValue = new float4(blend.lastValue, add.lastValue, 0, 0);
            var timeSpan = new float4(blend.timeSpan, add.timeSpan, 0, 0);
            var delay = new float4(blend.delay, add.delay, 0, 0);

            em.SetComponentData(mainEntity,
                new BillBoad.AlphaFadeData
                {
                    xBlend_yAdd = new BillBoad.Animation4Unit
                    {
                        Current = firstValue,
                        Min = math.min(firstValue, lastValue),
                        Max = math.max(firstValue, lastValue),
                        SpeedPerSec = new float4(((lastValue - firstValue) / timeSpan).xy, 0, 0),
                        Delay = delay,
                    }
                }
            );
        }

        //public static void AddBlendAlphaFadeComponents(
        //    this GameObjectConversionSystem gcs, GameObject main,
        //    float firstValue, float lastValue, float timeSpan, float delay)
        //{
        //    var em = gcs.DstEntityManager;

        //    var mainEntity = gcs.GetPrimaryEntity(main);

        //    var types = new ComponentTypes(
        //        typeof(BillBoad.BlendAlphaFadeData)
        //    );
        //    em.AddComponents(mainEntity, types);

        //    em.SetComponentData(mainEntity,
        //        new BillBoad.BlendAlphaFadeData
        //        {
        //            Fader = new BillBoad.AnimationUnit
        //            {
        //                Current = firstValue,
        //                Min = math.min(firstValue, lastValue),
        //                Max = math.max(firstValue, lastValue),
        //                SpeedPerSec = (lastValue - firstValue) / timeSpan,
        //                Delay = delay,
        //            }
        //        }
        //    );
        //}

        //public static void AddAdditiveAlphaFadeComponents(
        //    this GameObjectConversionSystem gcs, GameObject main,
        //    float firstValue, float lastValue, float timeSpan, float delay)
        //{
        //    var em = gcs.DstEntityManager;

        //    var mainEntity = gcs.GetPrimaryEntity(main);

        //    var types = new ComponentTypes(
        //        typeof(BillBoad.AdditiveAlphaFadeData)
        //    );
        //    em.AddComponents(mainEntity, types);

        //    em.SetComponentData(mainEntity,
        //        new BillBoad.AdditiveAlphaFadeData
        //        {
        //            Fader = new BillBoad.AnimationUnit
        //            {
        //                Current = firstValue,
        //                Min = math.min(firstValue, lastValue),
        //                Max = math.max(firstValue, lastValue),
        //                SpeedPerSec = (lastValue - firstValue) / timeSpan,
        //                Delay = delay,
        //            }
        //        }
        //    );
        //}


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

            em.SetComponentData(mainEntity,
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


            em.SetComponentData(mainEntity,
                new Particle.EasingData
                {
                    LastPosition = useDirectionSetting ? dir : 0,
                    Rate = rate,
                }
            );
            em.SetComponentData(mainEntity,
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
                typeof(Spring.StatesData),
                typeof(Spring.StickyStateData)
                //typeof(Spring.StickySelfFirstTag),//
                //typeof(Spring.StickyApplyData)
            );
            em.AddComponents(mainEntity, types);

            em.SetComponentData(mainEntity,
                new Spring.SpecData
                {
                    Spring = spring,
                    Dumper = dumper,
                    Rest = rest,
                    GravityFactor = 0.3f,
                }
            );
            em.SetComponentData(mainEntity,
                new Spring.StickyStateData
                {
                    NextSticky = Spring.NextStickyMode.first,
                    PointLength = segments + 1,
                }
            );
            //em.SetComponentData(mainEntity,
            //    new Spring.StickyApplyData
            //    {
            //        FirstFactor = 0.0f,
            //        LastFactor = 1.0f,
            //    }
            //);

            var buffer = em.AddBuffer<Spring.StatesData>(mainEntity);
            buffer.Length = segments + 1;
        }

    }
}