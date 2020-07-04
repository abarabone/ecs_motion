using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Particle.Aurthoring
{
    using Model;
    using Draw;
    using Model.Authoring;
    using Draw.Authoring;

    /// <summary>
    /// 
    /// </summary>
    public class MeshModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {


        public Material Material;



        /// <summary>
        /// 
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            
            createModelEntity_( conversionSystem, this.gameObject, this.Material );
            
            initInstanceEntityComponents_( conversionSystem, this.gameObject );
            
            return;


            void createModelEntity_
                ( GameObjectConversionSystem gcs, GameObject main, Material srcMaterial )
            {
                var mat = new Material( srcMaterial );
                var mesh = main.GetComponentInChildren<MeshFilter>().sharedMesh;

                const Draw.BoneType boneType = Draw.BoneType.TR;
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents( main, mesh, mat, boneType, boneLength );
            }

            void initInstanceEntityComponents_( GameObjectConversionSystem gcs, GameObject main )
            {
                dstManager.SetName( entity, $"{this.name}" );

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity( main );

                var archetype = em.CreateArchetype(
                    typeof( ModelPrefabNoNeedLinkedEntityGroupTag ),
                    typeof( DrawInstance.ModeLinkData ),
                    typeof( DrawInstance.TargetWorkData ),
                    typeof( Translation ),
                    typeof( Rotation ),
                    typeof( NonUniformScale )
                );
                em.SetArchetype( mainEntity, archetype );


                em.SetComponentData( mainEntity,
                    new DrawInstance.ModeLinkData
                    //new DrawTransformLinkData
                    {
                        DrawModelEntity = gcs.GetFromModelEntityDictionary( main ),
                    }
                );
                em.SetComponentData( mainEntity,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                em.SetComponentData( mainEntity,
                    new Translation
                    {
                        Value = float3.zero,
                    }
                );
                em.SetComponentData( mainEntity,
                    new Rotation
                    {
                        Value = quaternion.identity,
                    }
                );
                em.SetComponentData(mainEntity,
                    new NonUniformScale
                    {
                        Value = new float3(1.0f, 1.0f, 1.0f),
                    }
                ); ;
            }

        }


    }
    
}
