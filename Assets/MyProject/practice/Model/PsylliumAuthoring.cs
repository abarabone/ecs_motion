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
    public class PsylliumAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {


        public Material Material;



        /// <summary>
        /// 
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            
            createModelEntity_( conversionSystem, dstManager, this.gameObject, this.Material );
            
            initParticleEntityComponents_( conversionSystem, dstManager, this.gameObject );
            
            return;


            void createModelEntity_
                ( GameObjectConversionSystem gcs, EntityManager em, GameObject main, Material srcMaterial )
            {
                var mat = new Material( srcMaterial );
                var mesh = createMesh();

                const Draw.BoneType boneType = Draw.BoneType.TR;
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents( em, main, mesh, mat, boneType, boneLength );
            }

            void initParticleEntityComponents_
                ( GameObjectConversionSystem gcs, EntityManager em, GameObject main )
            {
                dstManager.SetName( entity, $"{this.name}" );

                var mainEntity = gcs.GetPrimaryEntity( main );

                var archetype = em.CreateArchetype(
                    typeof( ModelPrefabNoNeedLinkedEntityGroupTag ),
                    typeof( ParticleTag ),
                    typeof( DrawInstanceModeLinkData ),
                    typeof( DrawInstanceTargetWorkData ),
                    typeof( Translation ),
                    typeof( Rotation )
                );
                em.SetArchetype( mainEntity, archetype );

                em.SetComponentData( mainEntity,
                    new DrawInstanceModeLinkData
                    //new DrawTransformLinkData
                    {
                        DrawModelEntity = gcs.GetSingleton<ModelEntityDictionary.Data>().ModelDictionary[ main ],
                    }
                );
                em.SetComponentData( mainEntity,
                    new DrawInstanceTargetWorkData
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
            }

        }

        Mesh createMesh()
        {

            float height = 1.0f;
            float width = 1.0f;
            float radius = width;

            Mesh mesh = new Mesh();
            mesh.name = "psyllium";

            mesh.vertices = new Vector3[]
            {
            new Vector3 (-width, -height, -radius),     // 0
            new Vector3 (-width, -height, 0),           // 1
            new Vector3 (width , -height, -radius),     // 2
            new Vector3 (width , -height, 0),           // 3

            new Vector3 (-width,  height, 0),           // 4
            new Vector3 ( width,  height, 0),           // 5

            new Vector3 (-width,  height, radius),      // 6 
            new Vector3 (width ,  height, radius),      // 7

                //new Vector3 (-width,  height, -radius),     // 8
                //new Vector3 (width ,  height, -radius),     // 9
                //new Vector3 (-width, -height, radius),      // 10
                //new Vector3 (width , -height, radius),      // 11
            };

            mesh.uv = new Vector2[]
            {
            new Vector2 (0, 0),
            new Vector2 (0, 0.5f),
            new Vector2 (1, 0),
            new Vector2 (1, 0.5f),
            new Vector2 (0, 0.5f),
            new Vector2 (1, 0.5f),
            new Vector2 (0, 1),
            new Vector2 (1, 1),

                //new Vector2 (0, 0),
                //new Vector2 (1, 0),
                //new Vector2 (0, 1),
                //new Vector2 (1, 1),
            };

            //mesh.colors = new Color[]
            //{
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),

            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //    new Color (0, 0, 0, 0),
            //};

            mesh.triangles = new int[]
            {
            0, 1, 2,
            1, 3, 2,
            1, 4, 3,
            4, 5, 3,
            4, 6, 5,
            6, 7, 5,

                // 8, 4, 9,
                // 4, 5, 9,
                // 1,10, 3,
                //10,11, 3
            };

            return mesh;
        }

    }
    
}
