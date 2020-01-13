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

        Entity[] positions;


        public override Entity Convert
            ( EntityManager em, Func<Mesh, Material, BoneType, Entity> initDrawModelComponentsFunc )
        {

            var mesh = createMesh( pointNodeLength: this.Segment + 1 );
            var mat = new Material( this.Material );


            var drawModelEntity = initDrawModelComponentsFunc( mesh, mat, BoneType.T );

            var drawInstanceEntity = createDrawEntity_( em, drawModelEntity );

            var nodeLength = this.Segment + 1;
            var nodeEnitities = Enumerable.Range( 0, nodeLength )
                .Select( i => createDrawNodeEntity_( em, i, nodeLength, drawInstanceEntity, drawModelEntity ) )
                ;

            em.SetLinkedEntityGroup( drawInstanceEntity, nodeEnitities );

            //this.positions = nodeEnitities.ToArray();//
            //Enumerable.Repeat( new GameObject(), this.positions.Length ).ForEach( go => go.transform.parent = this.transform );//
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


        Mesh createMesh( int pointNodeLength )
        {

            const float h = 0.5f;
            const float w = 0.5f;
            const float d = 0.5f;

            Mesh mesh = new Mesh();


            var startEdgeVtxs   = new[] { new Vector3( -w, 0f, -d ), new Vector3( -w, 0f, -d ) };
            var nodeVtxs        = new[] { new Vector3( -w, 0f, 0f ), new Vector3( +w, 0f, 0f ) };
            var endEdgeVtxs     = new[] { new Vector3( -w, 0f, +d ), new Vector3( -w, 0f, +d ) };

            var qVtx = Enumerable
                .Repeat( nodeVtxs, pointNodeLength )
                .Prepend( startEdgeVtxs )
                .Append( endEdgeVtxs )
                ;


            var startEdgeUvs    = new[] { new Vector2( 0.0f, 0.0f ), new Vector2( 1.0f, 0.0f ) };
            var nodeUvs         = new[] { new Vector2( 0.0f, 0.5f ), new Vector2( 1.0f, 0.5f ) };
            var endEdgeUvs      = new[] { new Vector2( 0.0f, 1.0f ), new Vector2( 1.0f, 1.0f ) };

            var qUv = Enumerable
                .Repeat( nodeUvs, pointNodeLength )
                .Prepend( startEdgeUvs )
                .Append( endEdgeUvs )
                ;


            var qDirIdx = Enumerable.Range( 1, pointNodeLength - 2 )
                .Select( i => new Color( i, i, i - 1, i + 1 ) )
                .Prepend( new Color( 0, 0, 1, 1 ) )
                .Prepend( new Color( 0, 0, 1, 1 ) )
                .Append( new Color( pointNodeLength - 1, pointNodeLength - 2, pointNodeLength - 1, pointNodeLength - 1 ) )
                .Append( new Color( pointNodeLength - 1, pointNodeLength - 2, pointNodeLength - 1, pointNodeLength - 1 ) )
                ;


            var planeTris = new[]
            {
                0, 2, 1,
                2, 3, 1,
            };

            var qPlane = Enumerable
                .Repeat( planeTris, 2 + pointNodeLength - 1 )
                .Select( ( tri, i ) => tri.Select( x => x + i * 2 ) )
                ;


            mesh.vertices = qVtx.SelectMany( x => x ).ToArray();
            mesh.uv = qUv.SelectMany( x => x ).ToArray();
            mesh.colors = qDirIdx.ToArray();
            mesh.triangles = qPlane.SelectMany( x => x ).ToArray();

            return mesh;
        }


        private void Update()
        {
            return;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            this.gameObject
                .Descendants()
                .Select( go => go.transform )
                .Zip( this.positions, (tf, ent) => (tf,ent) )
                .Do( x => em.SetComponentData( x.ent, new Translation { Value = x.tf.position } ) );
        }
    }
}
