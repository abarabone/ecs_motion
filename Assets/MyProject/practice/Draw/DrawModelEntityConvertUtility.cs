using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Draw.Authoring
{
    using Motion;
    using Draw;
    using Character;
    using Abarabone.Authoring;

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Geometry;

    static public class DrawModelEntityConvertUtility
    {

        static public Entity CreateDrawModelEntityComponents
            ( this EntityManager em, Mesh mesh, Material mat, BoneType boneType, int boneLength )
        {

            setShaderProps_( em, mat, mesh, boneLength );
            
            var drawModelEntity = createEntityAndInitComponents_( boneLength );
            
            return drawModelEntity;
            

            void setShaderProps_( EntityManager em_, Material mat_, Mesh mesh_, int boneLength_ )
            {
                var sys = em_.World.GetExistingSystem<DrawBufferManagementSystem>();
                var boneVectorBuffer = sys.GetSingleton<DrawSystemComputeTransformBufferData>().Transforms;
                mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer );

                mat_.SetInt( "BoneLengthEveryInstance", boneLength_ );

                //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要なので、ここではやらない
            }

            Entity createEntityAndInitComponents_( int boneLength_ )
            {
                var drawModelArchetype = em.CreateArchetype(
                    typeof( DrawModelBoneUnitSizeData ),
                    typeof( DrawModelInstanceCounterData ),
                    typeof( DrawModelInstanceOffsetData ),
                    typeof( DrawModelGeometryData ),
                    typeof( DrawModelComputeArgumentsBufferData )
                );
                var ent = em.CreateEntity( drawModelArchetype );

                em.SetComponentData( ent,
                    new DrawModelBoneUnitSizeData
                    {
                        BoneLength = boneLength_,
                        VectorLengthInBone = (int)boneType,
                    }
                );
                em.SetComponentData( ent,
                    new DrawModelGeometryData
                    {
                        Mesh = mesh,
                        Material = mat,
                    }
                );
                em.SetComponentData( ent,
                    new DrawModelComputeArgumentsBufferData
                    {
                        InstanceArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
                    }
                );

                return ent;
            }

        }



        // メッシュを結合する
        static public Mesh CombineAndConvertMesh( IEnumerable<Mesh> meshes, Transform[] bones )
        {
            var qCis =
                from mesh in meshes
                select new CombineInstance
                {
                    mesh = mesh
                };

            //return ChMeshConverter.ConvertToChMesh( smrs_.ElementAt( 0 ).sharedMesh, smrs_.ElementAt(0).bones );

            var dstmesh = new Mesh();
            var boneLength = meshes.First().bindposes.Length;

            // 後でちゃんとした結合に差し替えよう
            dstmesh.CombineMeshes( qCis.ToArray(), mergeSubMeshes: true, useMatrices: false );
            dstmesh.boneWeights = (
                from w in dstmesh.boneWeights
                select new BoneWeight
                {
                    boneIndex0 = w.boneIndex0 % boneLength,
                    boneIndex1 = w.boneIndex1 % boneLength,
                    boneIndex2 = w.boneIndex2 % boneLength,
                    boneIndex3 = w.boneIndex3 % boneLength,
                    weight0 = w.weight0,
                    weight1 = w.weight1,
                    weight2 = w.weight2,
                    weight3 = w.weight3,
                }
            )
            .ToArray();

            return ChMeshConverter.ConvertToChMesh( dstmesh, bones );
        }

    }

}
