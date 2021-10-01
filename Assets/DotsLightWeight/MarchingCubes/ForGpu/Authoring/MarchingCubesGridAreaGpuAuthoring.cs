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

namespace DotsLite.MarchingCubes.Gpu.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Model;
    using DotsLite.MarchingCubes;
    using DotsLite.MarchingCubes.Authoring;
    using DotsLite.Draw.Authoring;

    public class MarchingCubesGridAreaGpuAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public MarchingCubesDotGridGpuAuthoring GridPrefab;

        public MarchingCubeAsset MarchingCubesAsset;


        public int3 GridLength;

        public GridFillMode FillMode;

        public int MaxGrids;
        public int MaxCubeInstances;

        public Texture2D Texture;
        public Shader DrawCubeShader;
        public ComputeShader GridToCubesShader;

        //public bool IsMode2;
        //public bool IsParallel;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.GridPrefab.gameObject);
        }

        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initGridArea_(conversionSystem, entity);

            initModel_(conversionSystem, entity);

            return;


            void initModel_(GameObjectConversionSystem gcs, Entity ent)
            {
                var mesh = createMesh_();
                var mat = new Material(this.DrawCubeShader);
                mat.mainTexture = this.Texture;

                var boneLength = 1;
                var sort = DrawModel.SortOrder.acs;
                var boneType = BoneType.T;
                var dataLength = sizeof(NearGridIndex);
                gcs.InitDrawModelEntityComponents(this.gameObject, ent, mesh, mat, boneType, boneLength, sort, dataLength);

                return;


                static Mesh createMesh_()
                {
                    var mesh_ = new Mesh();
                    mesh_.name = "marching cube unit";

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

            void initGridArea_(GameObjectConversionSystem gcs_, Entity ent)
            {
                var em = gcs_.DstEntityManager;

                var types = new ComponentTypes(
                    new ComponentType[]
                    {
                        typeof(Gpu.DotGridArea.InitializeData),
                        //typeof(DotGridArea.LinkToGridData),
                        typeof(DotGridArea.InfoData),
                        typeof(DotGridArea.InfoWorkData),
                        //typeof(DotGridArea.OutputCubesData),
                        typeof(Gpu.DotGridArea.ResourceGpuModeData),
                        typeof(DotGridArea.DotGridPrefabData),
                        typeof(Rotation),
                        typeof(Translation)
                    }
                );
                em.AddComponents(ent, types);
                //if (this.IsMode2) em.AddComponent<DotGridArea.Mode2>(ent);


                //var wholeLength = this.GridLength + 2;
                //var totalSize = wholeLength.x * wholeLength.y * wholeLength.z;

                var mat = new Material(this.DrawCubeShader);
                mat.mainTexture = this.Texture;

                em.SetComponentData(ent,
                    new Gpu.DotGridArea.InitializeData
                    {
                        //FillMode = fillMode_,
                        CubeMaterial = mat,
                        GridToCubesShader = this.GridToCubesShader,
                        MaxCubeInstances = this.MaxCubeInstances,
                        MaxGrids = this.MaxGrids,
                    }
                );
                //em.SetComponentData(ent,
                //    new DotGridArea.LinkToGridData
                //    {
                //        Grids = allocGridArea_(totalSize, fillMode_),
                //    }
                //);
                em.SetComponentData(ent,
                    new DotGridArea.InfoData
                    {
                        GridLength = this.GridLength,
                        //GridWholeLength = wholeLength,
                    }
                );
                em.SetComponentData(ent,
                    new DotGridArea.InfoWorkData
                    {
                        //GridSpan = new int3(1, wholeLength.x * wholeLength.z, wholeLength.x),
                        GridSpan = new int3(1, this.GridLength.x * this.GridLength.z, this.GridLength.x),
                    }
                );
                //em.SetComponentData(ent,
                //    new DotGridArea.OutputCubesData
                //    {
                //        GridInstances = new UnsafeList<GridInstanceData>(this.MaxGridInstances, Allocator.Persistent),
                //        CubeInstances = new UnsafeList<CubeInstance>(this.MaxCubeInstances, Allocator.Persistent),
                //    }
                //);
                //em.SetComponentData(ent,
                //    DotGridArea.ResourceGpuModeData
                //        .Create(this.MaxCubeInstances, this.MaxGridInstances, mat, this.CubeAdjacentShader)
                //);
                em.SetComponentData(ent,
                    new Rotation
                    {
                        Value = this.transform.rotation,
                    }
                );
                em.SetComponentData(ent,
                    new Translation
                    {
                        Value = this.transform.position,
                    }
                );

                em.SetComponentData(ent, new DotGridArea.DotGridPrefabData
                {
                    Prefab = gcs_.GetPrimaryEntity(this.GridPrefab),
                });


                //UnsafeList<DotGrid32x32x32Unsafe> allocGridArea_(int totalSize_, GridFillMode fillMode)
                //{
                //    var buffer = new UnsafeList<DotGrid32x32x32Unsafe>(totalSize, Allocator.Persistent);
                //    buffer.length = totalSize_;

                //    //var gent = gcs_.GetPrimaryEntity(global_);
                //    //var defaultGrids = em.GetComponentData<MarchingCubeGlobalData>(gent).DefaultGrids;//em.GetBuffer<DotGridGlobal.DefualtGridData>(gent);
                //    //var defaultGrid = defaultGrids[(int)FillMode];//.GetDefaultGrid(fillMode);

                //    //for (var i = 0; i < totalSize; i++)
                //    //{
                //    //    buffer[i] = defaultGrid;
                //    //}
                    
                //    return buffer;
                //}
            }


        }

    }

}
