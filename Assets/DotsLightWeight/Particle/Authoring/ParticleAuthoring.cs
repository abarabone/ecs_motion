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



            //initParticleEntityComponents_(conversionSystem, this.gameObject, modelEntity, this);
            var div = (this.ModelSource.DivisionU, this.ModelSource.DivisionV).as;
            conversionSystem.InitParticleEntityComponents(this.gameObject, modelEntity, div, this.CellUsage, this.AnimationBaseIndex, this.ParticleColor, this.Radius);

            conversionSystem.AddAnimationComponents(this.gameObject, this.AnimationIndexLength, this.AnimationTimeSpan);

            conversionSystem.AddLifeTimeComponents(this.gameObject, this.LifeTimeSec);

            return;


            static void initParticleEntityComponents_(
                GameObjectConversionSystem gcs, GameObject main, Entity modelEntity, ParticleAuthoring param)
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
                        IndexOffset = param.AnimationBaseIndex,
                        UCellUsage = (byte)param.CellUsage.x,
                        VCellUsage = (byte)param.CellUsage.y,
                        UMask = (byte)(param.ModelSource.DivisionU - 1),
                        VShift = (byte)math.countbits((int)param.ModelSource.DivisionU - 1),
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
                        Color = param.ParticleColor,
                        Size = param.Radius,
                    }
                );
            }

        }

    }



}
