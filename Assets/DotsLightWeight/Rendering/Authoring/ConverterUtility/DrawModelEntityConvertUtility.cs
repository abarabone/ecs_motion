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
    using DotsLite.Draw;

    using Material = UnityEngine.Material;


    static public class DrawModelEntityConvertUtility
    {

        static public Entity CreateDrawModelEntityComponents(
            this GameObjectConversionSystem gcs, GameObject topGameObject,
            Mesh mesh, Material mat,
            BoneType BoneType, int boneLength,
            DrawModel.SortOrder order,// = DrawModel.SortOrder.none)
            int instanceDataVectorLength = 0)
        {
            var em = gcs.DstEntityManager;
            var ent = em.CreateEntity();
            return InitDrawModelEntityComponents(
                gcs, topGameObject, ent, mesh, mat, BoneType, boneLength, order, instanceDataVectorLength);
        }


        static public Entity InitDrawModelEntityComponents(
            this GameObjectConversionSystem gcs, GameObject topGameObject, Entity drawModelEntity,
            Mesh mesh, Material mat,
            BoneType BoneType, int boneLength,
            DrawModel.SortOrder order,// = DrawModel.SortOrder.none
            int instanceDataVectorLength = 0)
        {

            var em = gcs.DstEntityManager;

            setShaderProps_(em, mat, mesh, boneLength * (int)BoneType + instanceDataVectorLength);

            addComponents_(gcs, topGameObject, drawModelEntity, order != DrawModel.SortOrder.none);
            initInfomationData_(em, drawModelEntity, mesh.bounds, boneLength, BoneType, instanceDataVectorLength, order);
            initResourceData_(em, drawModelEntity, mat, mesh);

            return drawModelEntity;



            static void setShaderProps_(EntityManager em_, Material mat_, Mesh mesh_, int vectorLengthPerInstance)
            {
                var sys = em_.World.GetOrCreateSystem<DrawBufferManagementSystem>();
                var boneVectorBuffer = sys.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;

                mat_.SetBuffer("BoneVectorBuffer", boneVectorBuffer);

                mat_.SetInt("VectorLengthPerInstance", vectorLengthPerInstance);

                //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要なので、ここではやらない
            }


            static void addComponents_(GameObjectConversionSystem gcs, GameObject top, Entity drawModelEntity, bool useSort)
            {
                var em = gcs.DstEntityManager;


                var types = new List<ComponentType>
                {
                    typeof(DrawModel.BoneVectorSettingData),
                    typeof(DrawModel.InstanceCounterData),
                    typeof(DrawModel.VectorIndexData),
                    typeof(DrawModel.BoundingBoxData),
                    typeof(DrawModel.GeometryData),
                    typeof(DrawModel.ComputeArgumentsBufferData),
                    //typeof(DrawModel.InitializeTag),
                };
                if (useSort) types.Add(typeof(DrawModel.SortSettingData));
                em.AddComponents(drawModelEntity, new ComponentTypes(types.ToArray()));

                gcs.AddToModelEntityDictionary(top, drawModelEntity);


                em.SetName_(drawModelEntity, $"{top.name} model" );
            }


            void initInfomationData_(
                EntityManager em_, Entity ent_,
                Bounds bbox_, int boneLength_, BoneType BoneType_, int instanceDataVectorLength_, DrawModel.SortOrder order)
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
                    new DrawModel.VectorIndexData
                    {
                        OptionalVectorLengthPerInstance = instanceDataVectorLength_,
                    }
                );

                if (order != DrawModel.SortOrder.none)
                {
                    em_.SetComponentData(ent_, new DrawModel.SortSettingData { Order = order });
                }
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
                        InstancingArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
                    }
                );

            }
        }

        // いずれは InitializeTag 依存で初期化するようにした方がいいかも…
        // でも結局、mesh は authoring からもってこないとだめだろうから、authoring 依存にはなるよなぁ…
        // メッシュデータを blob に詰めるって手もあるかもだけど
        //public static void SetShaderProps(EntityManager em, Entity ent, Material mat, Mesh mesh, int vectorLengthPerInstance)
        //{
        //    var sys = em.World.GetOrCreateSystem<DrawBufferManagementSystem>();
        //    var boneVectorBuffer = sys.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;

        //    mat.SetBuffer("BoneVectorBuffer", boneVectorBuffer);
        //    mat.SetInt("VectorLengthPerInstance", vectorLengthPerInstance);
        //    //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要なので、ここではやらない


        //    em.SetComponentData(ent, new DrawModel.ComputeArgumentsBufferData
        //    {
        //        InstancingArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
        //    });
        //}


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
