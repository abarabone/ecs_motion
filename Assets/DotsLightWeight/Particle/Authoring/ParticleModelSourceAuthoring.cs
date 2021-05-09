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
    public class ParticleModelSourceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public Shader DrawShader;
        public Color ParticleColor;

        public uint2 Division;

        //public bool UseRoundMesh;
        //public bool UseCustomSize;

        public uint IndexStart;
        public length_define IndexLength;
        public enum length_define
        {
            _1 = 1 - 1,
            _2 = 2 - 1,
            _4 = 4 - 1,
            _8 = 8 - 1,
        }

        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createModelEntity_(conversionSystem, this.gameObject, this.DrawShader, this.createMesh());

            initParticleEntityComponents_(conversionSystem, this.gameObject);
            //initParticleCustomEntityComponents_(conversionSystem, this.gameObject);

            return;


            static void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject main, Shader shader, Mesh mesh)
            {
                var mat = new Material(shader);

                const BoneType BoneType = BoneType.T;
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
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(Particle.AdditionalData),
                    typeof(BillBoad.UvCursor),
                    typeof(BillBoad.UvCursorParam),
                    typeof(BillBoad.UvParam),
                    typeof(Translation)
                );
                em.SetArchetype(mainEntity, archetype);


                em.SetComponentData(mainEntity,
                    new DrawInstance.ModelLinkData
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
                        Size = 1.0f,//this.DefaultRadius,
                    }
                );
                em.SetComponentData(mainEntity,
                    new BillBoad.UvCursor
                    {
                        CurrentIndex = 0,
                    }
                );
                em.SetComponentData(mainEntity,
                    new BillBoad.UvCursorParam
                    {

                    }
                );
                em.SetComponentData(mainEntity,
                    new BillBoad.UvParam
                    {
                        Span = BillBoad.CalcSpan(this.Division),
                        //UMask =
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
