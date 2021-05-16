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

        //public uint2 Division;
        public uint2 CellUsage;

        //public bool UseRoundMesh;
        //public bool UseCustomSize;

        public int AnimationIndexStart;
        public length_define AnimationIndexLength;
        public enum length_define
        {
            length_1 = 1,
            length_2 = 2,
            length_4 = 4,
            length_8 = 8,
        }
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



            initParticleEntityComponents_(conversionSystem, this.gameObject, modelEntity);

            addLifeTime_(conversionSystem, entity, this.LifeTimeSec);

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
                    typeof(BillBoad.CursorToUvIndexData),
                    typeof(BillBoad.UvAnimationWorkData),
                    typeof(BillBoad.UvAnimationData),
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
                    new BillBoad.CursorToUvIndexData
                    {
                        IndexOffset = this.AnimationIndexStart,
                        UCellUsage = (byte)this.CellUsage.x,
                        VCellUsage = (byte)this.CellUsage.y,
                        UMask = (byte)(this.ModelSource.DivisionU - 1),
                        VShift = (byte)math.countbits((int)this.ModelSource.DivisionU - 1),
                    }
                );
                em.SetComponentData(mainEntity,
                    new BillBoad.UvAnimationData
                    {
                        TimeSpan = this.AnimationTimeSpan,
                        CursorAnimationMask = (byte)(this.AnimationIndexLength - 1),
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
