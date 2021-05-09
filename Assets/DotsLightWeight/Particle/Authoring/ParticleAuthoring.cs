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

    /// <summary>
    /// 他メッシュとのアトラス対応は後回し
    /// </summary>
    public class ParticleAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public Shader DrawShader;
        public Color ParticleColor;

        public int Division;

        //public bool IsRound;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createModelEntity_(conversionSystem, this.gameObject, this.DrawShader, this.createMesh);

            initParticleEntityComponents_(conversionSystem, this.gameObject);

            return;


            static void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject main, Shader shader, Func<Mesh> createMesh)
            {
                var mat = new Material(shader);
                var mesh = createMesh();

                const BoneType BoneType = BoneType.TR;
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents(main, mesh, mat, BoneType, boneLength);
            }

            void initParticleEntityComponents_(GameObjectConversionSystem gcs, GameObject main)
            {
                dstManager.SetName_(entity, $"{this.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var archetype = em.CreateArchetype(
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(DrawInstance.ParticleTag),
                    typeof(DrawInstance.ModeLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(Particle.AdditionalData),
                    typeof(Particle.TranslationPtoPData),
                    typeof(Translation)
                );
                em.SetArchetype(mainEntity, archetype);


                em.SetComponentData(mainEntity,
                    new DrawInstance.ModeLinkData
                    //new DrawTransform.LinkData
                    {
                        DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(main),
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
                        Color = this.ParticleColor,
                        Size = this.DefaultRadius,
                    }
                );

                //em.SetComponentData(mainEntity,
                //    new Translation
                //    {
                //        Value = float3.zero,
                //    }
                //);
                //em.SetComponentData(mainEntity,
                //    new Rotation
                //    {
                //        Value = quaternion.identity,
                //    }
                //);
            }

        }

        Mesh createMesh()
        {

            float height = 0.5f;// 1.0f;
            float width = 0.5f;// 1.0f;

            Mesh mesh = new Mesh();
            mesh.name = "particle";

            mesh.vertices = new Vector3[]
            {
                new Vector3 (-width, -height, 0),     // 0
                new Vector3 (-width, -height, 0),           // 1
                new Vector3 (width , -height, 0),     // 2
                new Vector3 (width , -height, 0),           // 3
            };

            mesh.uv = new Vector2[]
            {
                new Vector2 (0, 0),
                new Vector2 (0, 0.5f),
                new Vector2 (1, 0),
                new Vector2 (1, 0.5f),
            };

            mesh.triangles = new int[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            return mesh;
        }

    }

}
