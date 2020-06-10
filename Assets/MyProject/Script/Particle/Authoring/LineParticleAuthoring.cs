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

using Abarabone.Geometry;
using Abarabone.Utilities;
using Abarabone.Misc;
using Abarabone.Motion;
using Abarabone.Draw;
using Abarabone.Character;
using Abarabone.Common.Extension;
using Abarabone.Particle;

namespace Abarabone.Authoring
{

    [DisallowMultipleComponent]
    public class LineParticleAuthoring : DrawPrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        public int Segment;
        public bool IsHalfEdge;

        public Material Material;


        public override Entity Convert
            ( EntityManager em, Func<Mesh, Material, BoneType, int, Entity> initDrawModelComponentsFunc )
        {

            var pointNodeLength = this.Segment + 1;
            var mesh = createMesh( pointNodeLength, this.IsHalfEdge );
            var mat = new Material( this.Material );
            
            var drawModelEntity = initDrawModelComponentsFunc( mesh, mat, BoneType.T, pointNodeLength );


            var drawInstanceEntity = createDrawEntity_( em, drawModelEntity );
            
            var nodeEnitities = Enumerable.Range( 0, pointNodeLength )
                .Select( i => createDrawNodeEntity_( em, i, pointNodeLength, drawInstanceEntity, drawModelEntity ) )
                .ToArray();

            SetChainLink_( em, drawInstanceEntity, nodeEnitities );

            em.SetLinkedEntityGroup( drawInstanceEntity, nodeEnitities );

            return drawInstanceEntity;




            Entity createDrawEntity_( EntityManager em_, Entity drawModelEntity_ )
            {
                var ent = em.CreateEntity();
                em_.AddComponentData( ent, new Prefab { } );
                //em_.AddBuffer<LinkedEntityGroup>( ent );

                em_.AddComponentData( ent,
                    new DrawInstanceModeLinkData
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

                em_.AddComponentData( ent,
                    new LineParticlePointNodeLinkData
                    {
                        NextNodeEntity = Entity.Null,
                    }
                );


                return ent;
            }

            Entity createDrawNodeEntity_
                ( EntityManager em_, int nodeId_, int pointNodeLength_, Entity drawInstanceEntity_, Entity drawModelEntity_ )
            {
                var ent = em_.CreateEntity();
                em_.AddComponentData( ent, new Prefab { } );

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

            void SetChainLink_( EntityManager em_, Entity instanceEntity_, IEnumerable<Entity> nodeEntities_ )
            {

                setLink_( instanceEntity_, nodeEntities_.First() );

                foreach( var x in (nodeEntities_, nodeEntities_.Skip(1)).Zip((x,y)=>(cur:x,next:y)) )
                {
                    setLink_( x.cur, x.next );
                }

                void setLink_( Entity current_, Entity next_ )
                {
                    var linker = em_.GetComponentData<LineParticlePointNodeLinkData>( current_ );
                    linker.NextNodeEntity = next_;
                    em_.SetComponentData( current_, linker );
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isHalfEdge">真ならＺ型、偽ならエ型のメッシュを生成する。</param>
        Mesh createMesh( int pointNodeLength, bool isHalfEdge )
        {

            const float h = 0.5f;
            const float w = 0.5f;
            const float d = 0.5f;

            Mesh mesh = new Mesh();
            
            mesh.vertices = queryVtx().SelectMany().ToArray();
            mesh.uv = queryUv().SelectMany().ToArray();
            mesh.colors = queryPointNodeIndex().SelectMany().ToArray();
            mesh.triangles = queryTriangleIndex().SelectMany().ToArray();

            return mesh;



            IEnumerable<Vector3[]> queryVtx()
            {
                var startEdgeVtxs = new[] { new Vector3( -w, 0f, -d ), new Vector3( +w, 0f, -d ) };
                var nodeVtxs = new[] { new Vector3( -w, 0f, 0f ), new Vector3( +w, 0f, 0f ) };
                var endEdgeVtxs = new[] { new Vector3( -w, 0f, +d ), new Vector3( +w, 0f, +d ) };

                var qVtx = Enumerable
                    .Repeat( nodeVtxs, pointNodeLength )
                    .Prepend( startEdgeVtxs )
                    .Append( endEdgeVtxs )
                    ;

                if( isHalfEdge )
                {
                    return qVtx;
                }
                else
                {
                    return qVtx
                        .Prepend( endEdgeVtxs )
                        .Append( startEdgeVtxs )
                        //.Select( (x,i) => new[] { x.First() + Vector3.up*i, x.Last() + Vector3.up*i } )
                        ;
                }
            }
            
            IEnumerable<Vector2[]> queryUv()
            {
                var startEdgeUvs = new[] { new Vector2( 0.0f, 0.0f ), new Vector2( 1.0f, 0.0f ) };
                var nodeUvs = new[] { new Vector2( 0.0f, 0.5f ), new Vector2( 1.0f, 0.5f ) };
                var endEdgeUvs = new[] { new Vector2( 0.0f, 1.0f ), new Vector2( 1.0f, 1.0f ) };

                var qUv = Enumerable
                    .Repeat( nodeUvs, pointNodeLength )
                    .Prepend( startEdgeUvs )
                    .Append( endEdgeUvs )
                    ;

                if( isHalfEdge )
                {
                    return qUv;
                }
                else
                {
                    return qUv
                        .Prepend( endEdgeUvs )
                        .Append( startEdgeUvs )
                        ;
                }
            }

            IEnumerable<Color[]> queryPointNodeIndex()
            {
                var lastNode = pointNodeLength - 1;
                var qNodeIdxSingle = Enumerable.Range( 1, pointNodeLength - 2 )
                    .Select( i => new Color( i, i, i - 1, 0 ) )
                    .Prepend( new Color( 0, 0, 0, 0 ) )
                    .Prepend( new Color( 0, 0, 0, 0 ) )
                    .Append( new Color( lastNode, lastNode - 1, lastNode - 1, 0 ) )
                    .Append( new Color( lastNode, lastNode - 1, lastNode - 1, 0 ) )
                    ;

                if( isHalfEdge )
                {
                    return ( qNodeIdxSingle, qNodeIdxSingle ).Zip( ( l, r ) => new[] { l, r } );
                }
                else
                {
                    var q = qNodeIdxSingle
                        .Prepend( new Color( 0, 0, 0, 0 ) )
                        .Append( new Color( lastNode, lastNode - 1, lastNode - 1, 0 ) )
                        ;
                    return (q, q).Zip( ( l, r ) => new[] { l, r } );
                }
            }

            IEnumerable<IEnumerable<int>> queryTriangleIndex()
            {
                var planeTris = new[]
                {
                    0, 2, 1,
                    2, 3, 1,
                };

                var qTri = Enumerable
                    .Repeat( planeTris, 2 + pointNodeLength - 1 )
                    ;

                if( isHalfEdge )
                {
                    return qTri
                        .Select( ( tri, i ) => tri.Select( x => x + i * 2 ) )
                        ;
                }
                else
                {
                    // ここはだいぶ難解になってしまった…
                    var firstPlaneTris = new[]
                    {
                        4, 0, 5,
                        0, 1, 5,
                    };
                    var lastPlaneTris = new[]
                    {
                        2, -2, 3,
                        -2, -1, 3,
                    };
                    return qTri
                        .Prepend( firstPlaneTris )
                        .Append( lastPlaneTris )
                        .Select( ( tri, i ) => tri.Select( x => x + i * 2 ) )
                        ;
                    ;
                }
            }
            
        }

    }
}
