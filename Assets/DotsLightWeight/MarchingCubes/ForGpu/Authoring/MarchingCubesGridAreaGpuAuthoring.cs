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

    public class MarchingCubesGridAreaGpuAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public int3 GridLength;

        public GridFillMode FillMode;

        public int MaxGrids;
        public int MaxCubeInstances;

        public Texture2D Texture;
        public Shader DrawCubeShader;
        public ComputeShader GridToCubesShader;

        //public bool IsMode2;
        //public bool IsParallel;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var global = this.GetComponentInParent<MarchingCubesGlobalAuthoring>().gameObject;//<MarchingCubesGlobalDataAuthoring>().gameObject;
            var area = this.gameObject;

            setGridArea_(conversionSystem, global, area, this.FillMode);

            return;


            void setGridArea_
                (GameObjectConversionSystem gcs_, GameObject global_, GameObject area_, GridFillMode fillMode_)
            {
                var em = gcs_.DstEntityManager;

                var ent = gcs_.GetPrimaryEntity(area_);
                var types = new ComponentTypes(
                    new ComponentType[]
                    {
                        typeof(Gpu.DotGridArea.InitializeData),
                        //typeof(DotGridArea.LinkToGridData),
                        typeof(DotGridArea.InfoData),
                        typeof(DotGridArea.InfoWorkData),
                        //typeof(DotGridArea.OutputCubesData),
                        //typeof(DotGridArea.ResourceGpuModeData),
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
