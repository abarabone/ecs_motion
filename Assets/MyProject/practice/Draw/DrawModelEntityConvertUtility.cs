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
    using Abarabone.Model.Authoring;

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Geometry;

    static public class DrawModelEntityConvertUtility
    {


        //static Dictionary<GameObject, Entity> dict = new Dictionary<GameObject, Entity>();

        //static void SetModelEntity( this GameObjectConversionSystem gcs, GameObject main, Entity entity ) => dict[ main ] = entity;
        //static Entity GetModelEntity( this GameObjectConversionSystem gcs, GameObject main ) => dict[ main ];



        static public Entity CreateDrawModelEntityComponents
            (
                this GameObjectConversionSystem gcs, GameObject mainGameObject,
                Mesh mesh, Material mat, BoneType boneType, int boneLength
            )
        {
            var em = gcs.DstEntityManager;

            setShaderProps_( em, mat, mesh, boneLength );

            var drawModelEntity = createDrawModelEntity_( gcs, mainGameObject );
            setEntityComponentValues_( em, drawModelEntity, mat, mesh, boneLength, boneType );

            return drawModelEntity;


            void setShaderProps_( EntityManager em_, Material mat_, Mesh mesh_, int boneLength_ )
            {
                var sys = em_.World.GetExistingSystem<DrawBufferManagementSystem>();
                var boneVectorBuffer = sys.GetSingleton<DrawSystemComputeTransformBufferData>().Transforms;
                mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer );

                mat_.SetInt( "BoneLengthEveryInstance", boneLength_ );

                //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要なので、ここではやらない
            }

            Entity createDrawModelEntity_
                ( GameObjectConversionSystem gcs_, GameObject main_ )
            {
                var em_ = gcs_.DstEntityManager;

                var drawModelArchetype = em_.CreateArchetype(
                    typeof( DrawModelBoneUnitSizeData ),
                    typeof( DrawModelInstanceCounterData ),
                    typeof( DrawModelInstanceOffsetData ),
                    typeof( DrawModelGeometryData ),
                    typeof( DrawModelComputeArgumentsBufferData )
                );
                var ent = em_.CreateEntity( drawModelArchetype );

                gcs.GetSingleton<ModelEntityDictionary.Data>().ModelDictionary.Add( main_, ent );

                em_.SetName( ent, $"{main_.name} model" );

                return ent;
            }

            void setEntityComponentValues_
                ( EntityManager em_, Entity ent_, Material mat_, Mesh mesh_, int boneLength_, BoneType boneType_ )
            {

                em_.SetComponentData( ent_,
                    new DrawModelBoneUnitSizeData
                    {
                        BoneLength = boneLength_,
                        VectorLengthInBone = (int)boneType_,
                    }
                );
                em_.SetComponentData( ent_,
                    new DrawModelGeometryData
                    {
                        Mesh = mesh_,
                        Material = mat_,
                    }
                );
                em_.SetComponentData( ent_,
                    new DrawModelComputeArgumentsBufferData
                    {
                        InstanceArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
                    }
                );

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
