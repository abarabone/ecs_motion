using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Draw.Authoring
{
    using DotsLite.CharacterMotion;
    using DotsLite.Draw;
    using DotsLite.Character;
    
    using DotsLite.Model.Authoring;

    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Geometry;
    using DotsLite.Model;
    using Unity.Physics;

    using Material = UnityEngine.Material;


    static public class DrawModelEntityConvertUtility
    {

        static public Entity CreateDrawModelEntityComponents(
            this GameObjectConversionSystem gcs, GameObject topGameObject,
            Mesh mesh, Material mat,
            BoneType BoneType, int boneLength, int instanceDataVectorLength = 0)
        {
            var em = gcs.DstEntityManager;
            var ent = em.CreateEntity();
            return InitDrawModelEntityComponents(gcs, topGameObject, ent, mesh, mat, BoneType, boneLength, instanceDataVectorLength);
        }


        static public Entity InitDrawModelEntityComponents(
            this GameObjectConversionSystem gcs, GameObject topGameObject, Entity drawModelEntity,
            Mesh mesh, Material mat,
            BoneType BoneType, int boneLength, int instanceDataVectorLength = 0)
        {

            var em = gcs.DstEntityManager;

            setShaderProps_( em, mat, mesh, boneLength );

            addComponents_( gcs, topGameObject, drawModelEntity);
            initInfomationData_( em, drawModelEntity, mesh.bounds, boneLength, BoneType, instanceDataVectorLength );
            initResourceData_(em, drawModelEntity, mat, mesh);

            return drawModelEntity;



            static void setShaderProps_( EntityManager em_, Material mat_, Mesh mesh_, int boneLength_ )
            {
                var sys = em_.World.GetExistingSystem<DrawBufferManagementSystem>();
                var boneVectorBuffer = sys.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;

                mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer );

                mat_.SetInt( "BoneLengthEveryInstance", boneLength_ );

                //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要なので、ここではやらない
            }


            static void addComponents_(GameObjectConversionSystem gcs, GameObject top, Entity drawModelEntity)
            {
                var em = gcs.DstEntityManager;


                var types = new ComponentTypes(new ComponentType[] {
                    typeof( DrawModel.BoneVectorSettingData ),
                    typeof( DrawModel.InstanceCounterData ),
                    typeof( DrawModel.InstanceOffsetData ),
                    typeof( DrawModel.BoundingBoxData ),
                    typeof( DrawModel.GeometryData ),
                    typeof( DrawModel.ComputeArgumentsBufferData )
                });
                em.AddComponents(drawModelEntity, types);

                gcs.AddToModelEntityDictionary(top, drawModelEntity);


                em.SetName_(drawModelEntity, $"{top.name} model" );
            }


            void initInfomationData_(
                EntityManager em_, Entity ent_,
                Bounds bbox_, int boneLength_, BoneType BoneType_, int instanceDataVectorLength_)
            {

                em_.SetComponentData(ent_,
                    new DrawModel.BoneVectorSettingData
                    {
                        BoneLength = boneLength_,
                        VectorLengthInBone = (int)BoneType_,
                    }
                );
                em_.SetComponentData(ent_,
                    new DrawModel.BoundingBoxData
                    {
                        localBbox = new AABB
                        {
                            Center = (float3)bbox_.center,// + meshpos_,
                            Extents = (float3)bbox_.extents,
                        }
                    }
                );
                em_.SetComponentData(ent_,
                    new DrawModel.InstanceOffsetData
                    {
                        VectorOffsetPerInstance = instanceDataVectorLength_,
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



        //// メッシュを結合する
        //static public Mesh CombineAndConvertMesh( IEnumerable<Mesh> meshes, Transform[] bones )
        //{
        //    var qCis =
        //        from mesh in meshes
        //        select new CombineInstance
        //        {
        //            mesh = mesh
        //        };

        //    //return ChMeshConverter.ConvertToChMesh( smrs_.ElementAt( 0 ).sharedMesh, smrs_.ElementAt(0).bones );

        //    var dstmesh = new Mesh();
        //    var boneLength = meshes.First().bindposes.Length;

        //    // 後でちゃんとした結合に差し替えよう
        //    dstmesh.CombineMeshes( qCis.ToArray(), mergeSubMeshes: true, useMatrices: false );
        //    dstmesh.boneWeights = (
        //        from w in dstmesh.boneWeights
        //        select new BoneWeight
        //        {
        //            boneIndex0 = w.boneIndex0 % boneLength,
        //            boneIndex1 = w.boneIndex1 % boneLength,
        //            boneIndex2 = w.boneIndex2 % boneLength,
        //            boneIndex3 = w.boneIndex3 % boneLength,
        //            weight0 = w.weight0,
        //            weight1 = w.weight1,
        //            weight2 = w.weight2,
        //            weight3 = w.weight3,
        //        }
        //    )
        //    .ToArray();

        //    return ChMeshConverter.ConvertToChMesh( dstmesh, bones );
        //}

    }

}
