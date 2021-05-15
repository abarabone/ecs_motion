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

        public Color ParticleColor;
        public float Radius;

        public uint2 Division;

        //public bool UseRoundMesh;
        //public bool UseCustomSize;

        public uint IndexStart;
        public length_define IndexLength;
        public enum length_define
        {
            length_1 = 1,
            length_2 = 2,
            length_4 = 4,
            length_8 = 8,
        }

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

            //var modelEntity = conversionSystem.GetPrimaryEntity(this.ModelSource);
            //dstManager.RemoveComponent<Prefab>(modelEntity);
            //var isExists = conversionSystem.IsExistsInModelEntityDictionary(this.ModelSource.gameObject);
            //if (!isExists)
            //{
            //    var ent = conversionSystem.GetPrimaryEntity(this.ModelSource);
            //    this.ModelSource.Convert(ent, dstManager, conversionSystem);
            //}
            var modelEntity = conversionSystem.GetPrimaryEntity(this.ModelSource);



            initParticleEntityComponents_(conversionSystem, this.gameObject, modelEntity);
            //initParticleCustomEntityComponents_(conversionSystem, this.gameObject);

            addLifeTime_(conversionSystem, entity, this.LifeTimeSec);
            //addDurationTime_(conversionSystem, entity, this.LifeTimeSec);

            return;


            void initParticleEntityComponents_(GameObjectConversionSystem gcs, GameObject main, Entity modelEntity)
            {
                gcs.DstEntityManager.SetName_(entity, $"{main.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var archetype = em.CreateArchetype(
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(DrawInstance.ParticleTag),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(BillBoad.UvCursorData),
                    typeof(BillBoad.UvCursorParam),
                    typeof(BillBoad.IndexToUvData),
                    typeof(BillBoad.RotationData),
                    typeof(Particle.AdditionalData),
                    typeof(Translation)
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
                    new BillBoad.UvCursorParam
                    {
                        IndexPrevMask = (int)(this.IndexLength - 1),
                        IndexAfterOffset = (int)this.IndexStart,
                    }
                );
                em.SetComponentData(mainEntity,
                    new BillBoad.IndexToUvData
                    {
                        Span = BillBoad.CalcSpan(this.Division),
                        UMask = BillBoad.CalcUMask(this.Division),
                        VShift = BillBoad.CalcVShift(this.Division),
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
                        Color = this.ParticleColor,
                        Size = this.Radius,
                    }
                );
            }

            //    void initParticleCustomEntityComponents_(GameObjectConversionSystem gcs, GameObject main)
            //    {
            //        dstManager.SetName_(entity, $"{this.name}");

            //        var em = gcs.DstEntityManager;


            //        var mainEntity = gcs.GetPrimaryEntity(main);

            //        var archetype = em.CreateArchetype(
            //            typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
            //            typeof(DrawInstance.ParticleTag),
            //            typeof(DrawInstance.ModelLinkData),
            //            typeof(DrawInstance.TargetWorkData),
            //            typeof(Particle.AdditionalData),
            //            typeof(BillBoadCustom.UvCursor),
            //            typeof(BillBoadCustom.UvInfo),
            //            typeof(Translation)
            //        );
            //        em.SetArchetype(mainEntity, archetype);


            //        em.SetComponentData(mainEntity,
            //            new DrawInstance.ModelLinkData
            //            //new DrawTransform.LinkData
            //            {
            //                DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(main),
            //            }
            //        );
            //        em.SetComponentData(mainEntity,
            //            new DrawInstance.TargetWorkData
            //            {
            //                DrawInstanceId = -1,
            //            }
            //        );
            //        em.SetComponentData(mainEntity,
            //            new Particle.AdditionalData
            //            {
            //                Color = this.ParticleColor,
            //                Size = 1.0f,//this.DefaultRadius,
            //            }
            //        );
            //        em.SetComponentData(mainEntity,
            //            new BillBoadCustom.UvCursor
            //            {
            //                CurrentId = 0,
            //                Length = 1,
            //            }
            //        );
            //        em.SetComponentData(mainEntity,
            //            new BillBoadCustom.UvInfo
            //            {
            //                Span = BillBoad.CalcSpan(this.Division),
            //            }
            //        );
            //    }

            //static void addDurationTime_(GameObjectConversionSystem gcs, Entity ent, float time)
            //{
            //    if (time > 0.0f) return;

            //    gcs.DstEntityManager.AddComponentData(ent, new Particle.DurationData
            //    {
            //        DurationSec = time,
            //    });
            //}

            static void addLifeTime_(GameObjectConversionSystem gcs, Entity ent, float time)
            {
                if (time <= 0.0f) return;

                var types = new ComponentTypes(
                    typeof(Particle.LifeTimeSpecData),
                    typeof(Particle.LifeTimeData)
                );
                gcs.DstEntityManager.AddComponents(ent, types);

                gcs.DstEntityManager.AddComponentData(ent, new Particle.LifeTimeSpecData
                {
                    DurationSec = time,
                });
            }
        }

    }



}
