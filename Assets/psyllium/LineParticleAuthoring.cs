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




public struct LineParticlePointNodeLinkData : IComponentData
{
    public Entity NextNodeEntity;
}

namespace Abss.Arthuring
{

    [DisallowMultipleComponent]
    public class LineParticleAuthoring : DrawPrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        public int Segment;

        public Material Material;


        public override Entity Convert
            ( EntityManager em, Func<Mesh, Material, BoneType, Entity> initDrawModelComponentsFunc )
        {

            var mesh = createMesh( this.Segment );
            var mat = new Material( this.Material );


            var drawModelEntity = initDrawModelComponentsFunc( mesh, mat, BoneType.T );

            var drawInstanceEntity = createDrawEntity_( em, drawModelEntity );

            var nodeLength = this.Segment + 1;
            var nodeEnitities = Enumerable.Range( 0, nodeLength )
                .Select( i => createDrawNodeEntity_( em, i, nodeLength, drawInstanceEntity, drawModelEntity ) )
                ;

            em.SetLinkedEntityGroup( drawInstanceEntity, nodeEnitities );

            return drawInstanceEntity;


            Entity createDrawEntity_( EntityManager em_, Entity drawModelEntity_ )
            {
                var ent = em.CreateEntity();

                em_.AddComponentData( ent,
                    new DrawInstanceIndexOfModelData
                    {
                        DrawModelEntity = drawModelEntity_
                    }
                );
                em_.AddComponentData( ent,
                    new DrawInstanceTargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                //em_.AddBuffer<LinkedEntityGroup>( ent );
                em_.AddComponentData( ent, new Prefab { } );

                return ent;
            }

            Entity createDrawNodeEntity_
                ( EntityManager em_, int nodeId_, int pointNodeLength_, Entity drawInstanceEntity_, Entity drawModelEntity_ )
            {
                var ent = em.CreateEntity();

                em_.AddComponentData( ent,
                    new LineParticlePointNodeLinkData
                    {
                        NextNodeEntity = Entity.Null,
                    }
                );

                em_.AddComponentData( ent,
                    new DrawTransformLinkData
                    {
                        DrawInstanceEntity = drawInstanceEntity_,
                        DrawModelEntity = drawModelEntity_,
                    }
                );
                em_.AddComponentData( ent,
                    new DrawTransformIndexData
                    {
                        BoneId = nodeId_,
                        BoneLength = pointNodeLength_,
                    }
                );
                em_.AddComponentData( ent,
                    new DrawTransformTargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                em_.AddComponentData( ent, new Translation { } );

                return ent;
            }

        }


        Mesh createMesh( int segmentLength )
        {

            float h = 1.0f;
            float w = 1.0f;
            float r = w;

            Mesh mesh = new Mesh();


            var planeVertics = new Vector3[]
            {
                new Vector3(-w, -h, 0),
                new Vector3(w, -h, 0),
                new Vector3(-w, h, 0),
                new Vector3(w, h, 0),
            };
            var planeTriangles = new int[]
            {
                0, 1, 2,
                2, 3, 1,
            };


            var nodeLength = segmentLength + 1;

            var qLeftVertices = Enumerable.Range( 0, nodeLength )
                .Select( i => new Vector3( -w, i, 0 ) )
                ;
            var qRightVertices = Enumerable.Range( 0, nodeLength )
                .Select( i => new Vector3( w, i, 0 ) )
                ;
            var qLineVertices = (qLeftVertices, qRightVertices).Zip()
                .Select( x => new[] { x.x, x.y } )
                .Concat()
                ;

            var qPlaneTris = Enumerable.Repeat( planeTriangles, segmentLength )
                .SelectMany( ( xs, i ) => from x in xs select x + i * 4 )
                ;

            var qLeftUvs = Enumerable.Range( 0, nodeLength )
                .Select( i => new Vector4( 0, 0.5f,  ) )
                ;
            var qRightUvs = Enumerable.Range( 0, nodeLength )
                .Select( i => new Vector4( 1, 0.5f ) )
                ;
            var qLineUvs = (qLeftUvs, qRightUvs).Zip()
                .Select( x => new[] { x.x, x.y } )
                .Concat()
                ;


            mesh.vertices = qLineVertices.ToArray();
            mesh.triangles = qPlaneTris.ToArray();
            mesh.SetUVs( 0, qLineUvs.ToArray() );

            return mesh;
        }

    }
}
