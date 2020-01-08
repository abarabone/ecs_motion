using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;
using Unity.Mathematics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Character;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    [DisallowMultipleComponent]
    public class PsylliumAuthoring : DrawPrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        public Material Material;


        public override Entity Convert
            ( EntityManager em, Func<Mesh, Material, BoneType, Entity> initDrawModelComponentsFunc )
        {

            var mesh = createMesh();
            var mat = this.Material;


            var ent = em.CreateEntity();
            var modelEntity = initDrawModelComponentsFunc( mesh, mat, BoneType.TR );

            em.AddComponentData( ent, new Prefab { } );

            em.AddComponentData( ent,
                new DrawIndexOfModelData
                {
                    ModelEntity = modelEntity,
                    BoneLength = 1,
                }
            );
            em.AddComponentData( ent,
                new DrawInstanceTargetWorkData
                {
                    InstanceIndex = -1,
                }
            );

            em.AddComponentData( ent,
                new Translation
                {
                    Value = float3.zero,
                }
            );
            em.AddComponentData( ent,
                new Rotation
                {
                    Value = quaternion.identity,
                }
            );


            return ent;
        }


        Mesh createMesh()
        {

            float height = 1.0f;
            float width = 1.0f;
            float radius = width;

            Mesh mesh = new Mesh();
            
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

                new Vector3 (-width,  height, -radius),     // 8
                new Vector3 (width ,  height, -radius),     // 9
                new Vector3 (-width, -height, radius),      // 10
                new Vector3 (width , -height, radius),      // 11
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

                new Vector2 (0, 0),
                new Vector2 (1, 0),
                new Vector2 (0, 1),
                new Vector2 (1, 1),
            };

            //mesh.uv2 = new Vector2[]
            //{
            //    new Vector2 (-radius, 0),
            //    new Vector2 (0, 0),
            //    new Vector2 (-radius, 0),
            //    new Vector2 (0, 0),
            //    new Vector2 (0, 0),
            //    new Vector2 (0, 0),
            //    new Vector2 (radius, 0),
            //    new Vector2 (radius, 0),

            //    new Vector2 (-radius, 0),
            //    new Vector2 (-radius, 0),
            //    new Vector2 (radius, 0),
            //    new Vector2 (radius, 0),
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
