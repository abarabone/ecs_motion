using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.MarchingCubes.another.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.MarchingCubes.another.Data;
    using DotsLite.MarchingCubes.another.Data.Resource;

    public class MarchingCubesDrawModelAuthoring : MonoBehaviour
    {

        public MarchingCubesAsset MarchingCubesAsset;

        public int MaxGrids;
        public int MaxCubeInstances;

        public Texture2D Texture;
        public Shader DrawCubeShader;
        public ComputeShader GridToCubesShader;



        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            init_(conversionSystem, entity);

            initModel_(conversionSystem, entity, this.GridToCubesShader);

            return;


            void initModel_(GameObjectConversionSystem gcs, Entity ent, ComputeShader cs)
            {
                var mesh = createMesh_();
                var mat = new Material(this.DrawCubeShader);
                mat.mainTexture = this.Texture;

                initDrawModel_(ent, mat, mesh);
                setComputeShaderParametor_(cs);
                return;


                void initDrawModel_(Entity ent, Material mat, Mesh mesh)
                {
                    var boneLength = 1;
                    var sort = DrawModel.SortOrder.acs;
                    var boneType = BoneType.T;
                    var dataLength = sizeof(NearGridIndex);
                    gcs.InitDrawModelEntityComponents(
                        this.gameObject, ent, mesh, mat, boneType, boneLength, sort, dataLength);
                }

                void setComputeShaderParametor_(ComputeShader cs)
                {
                    var em = gcs.DstEntityManager;
                    var sys = em.World.GetExistingSystem<DrawBufferManagementSystem>();
                    var boneVectorBuffer = sys.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;
                    cs.SetInt("VectorLengthPerInstance", 3);
                    cs.SetBuffer(0, "BoneVectorBuffer", boneVectorBuffer);
                }

                static Mesh createMesh_()
                {
                    var mesh_ = new Mesh();
                    mesh_.name = "marching cube template";

                    var qVtx =
                        from i in Enumerable.Range(0, 12)
                        select new Vector3(i % 3, i / 3, 0)
                        ;
                    var qIdx =
                        from i in Enumerable.Range(0, 3 * 4)
                        select i
                        ;
                    mesh_.vertices = qVtx.ToArray();
                    mesh_.triangles = qIdx.ToArray();

                    return mesh_;
                }
            }
        }

        void init_(GameObjectConversionSystem gcs_, Entity ent)
        {
            var em = gcs_.DstEntityManager;

            var types = new ComponentTypes(
                new ComponentType[]
                {
                    typeof(DrawModel.ExcludeDrawMeshCsTag),
                    typeof(CubeDrawModel.GridTypeData),
                    //typeof(DotGridArea.OutputCubesData),
                    typeof(CubeDrawModel.MakeCubesShaderResourceData),
                }
            );
            em.AddComponents(ent, types);


            em.SetComponentData(ent, new CubeDrawModel.MakeCubesShaderResourceData
            {
                CubeIds = GridCubeIdShaderBufferTexture.Create(this.MaxCubeInstances, ),
                CubeInstances = CubeInstancingShaderBuffer.Create(this.MaxCubeInstances),
                DotContents = Data.Resource.GridContentDataBuffer.Create(this.MaxGrids, ),
                MakeCubesShader = this.GridToCubesShader,
            });
        }

    }
}