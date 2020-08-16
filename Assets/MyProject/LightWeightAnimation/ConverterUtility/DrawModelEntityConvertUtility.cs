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
    using Abarabone.CharacterMotion;
    using Abarabone.Draw;
    using Abarabone.Character;
    using Abarabone.Authoring;
    using Abarabone.Model.Authoring;

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Model;
    using Unity.Physics;

    using Material = UnityEngine.Material;
    using System.CodeDom;

    static public class DrawModelEntityConvertUtility
    {



        static public Entity CreateDrawModelEntityComponents
            (
                this GameObjectConversionSystem gcs, GameObject topGameObject,
                Mesh mesh, Material mat,
                BoneType boneType, int boneLength, int instanceDataVectorLength = 0
            )
        {

            var em = gcs.DstEntityManager;

            setShaderProps_( em, mat, mesh, boneLength );

            var drawModelEntity = createDrawModelEntity_( gcs, topGameObject );
            initInfomationData_( em, drawModelEntity, mesh.bounds, boneLength, boneType, instanceDataVectorLength );
            initResourceData_(em, drawModelEntity, mat, mesh);

            return drawModelEntity;



            void setShaderProps_( EntityManager em_, Material mat_, Mesh mesh_, int boneLength_ )
            {
                var sys = em_.World.GetExistingSystem<DrawBufferManagementSystem>();
                var boneVectorBuffer = sys.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;
                mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer );

                mat_.SetInt( "BoneLengthEveryInstance", boneLength_ );

                //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要なので、ここではやらない
            }


            Entity createDrawModelEntity_
                ( GameObjectConversionSystem gcs_, GameObject top_ )
            {
                var em_ = gcs_.DstEntityManager;


                var drawModelArchetype = em_.CreateArchetype(
                    typeof( DrawModel.BoneVectorSettingData ),
                    typeof( DrawModel.InstanceCounterData ),
                    typeof( DrawModel.VectorBufferData ),
                    typeof( DrawModel.VectorLengthData ),
                    typeof( DrawModel.BoundingBoxData ),
                    typeof( DrawModel.GeometryData ),
                    typeof( DrawModel.ComputeArgumentsBufferData )
                );
                var ent = em_.CreateEntity( drawModelArchetype );

                gcs.AddToModelEntityDictionary(top_, ent);


                em_.SetName_( ent, $"{top_.name} model" );

                return ent;
            }


            void initInfomationData_
                (
                    EntityManager em_, Entity ent_,
                    Bounds bbox_, int boneLength_, BoneType boneType_, int instanceDataVectorLength_
                )
            {

                em_.SetComponentData(ent_,
                    new DrawModel.BoneVectorSettingData
                    {
                        BoneLength = boneLength_,
                        VectorLengthInBone = (int)boneType_,
                    }
                );
                em_.SetComponentData(ent_,
                    new DrawModel.BoundingBoxData
                    {
                        LocalBbox = new AABB
                        {
                            Center = (float3)bbox_.center,// + meshpos_,
                            Extents = (float3)bbox_.extents,
                        }
                    }
                );
                em_.SetComponentData(ent_,
                    new DrawModel.VectorLengthData
                    {
                        VectorLengthOfInstanceAdditionalData = instanceDataVectorLength_,
                        VecotrLengthPerInstance = (int)boneType_ * boneLength_ + instanceDataVectorLength_,
                    }
                );
            }

            void initResourceData_
                ( EntityManager em_, Entity ent_, Material mat_, Mesh mesh_ )
            {

                em_.SetComponentData( ent_,
                    new DrawModel.GeometryData
                    {
                        Mesh = mesh_,
                        Material = mat_,
                    }
                );
                em_.SetComponentData( ent_,
                    new DrawModel.ComputeArgumentsBufferData
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
