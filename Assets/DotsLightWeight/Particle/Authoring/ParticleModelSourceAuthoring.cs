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
        public Texture2D Texture;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, this.createMesh(), this.Texture);

            return;


            static void createModelEntity_
                (GameObjectConversionSystem gcs, Entity entity, GameObject main, Shader shader, Mesh mesh, Texture tex)
            {
                var mat = new Material(shader);
                mat.mainTexture = tex;

                const BoneType BoneType = BoneType.T;
                const int boneLength = 1;
                const int additional = 1;

                gcs.InitDrawModelEntityComponents(main, entity, mesh, mat, BoneType, boneLength, additional);
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
