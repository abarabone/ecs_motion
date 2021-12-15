using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics.Authoring;

namespace DotsLite.HeightGrid.Aurthoring
{
    using DotsLite.HeightGrid;
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Misc;
    using DotsLite.Particle.Aurthoring;

    public class HeightGridWithAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {

        //public float UnitDistance;
        //public GridBinaryLength2 UnitLengthInGrid;
        public BinaryLength2 NumGrids;

        public int MaxLodLevel;

        public Shader DrawShader;
        public Texture Texture;

        public bool UseHalfSlantMesh;

        [SerializeField] PhysicsCategoryTags belongsTo;
        [SerializeField] PhysicsCategoryTags collidesWith;
        //[SerializeField] int groupIndex;


        //public ParticleAuthoringBase SplashPrefab;


        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    referencedPrefabs.Add(this.SplashPrefab.gameObject);
        //}


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.gameObject.activeInHierarchy) return;

            var terrainData = getTrrainData_();
            if (terrainData == null) return;

            var gcs = conversionSystem;
            var em = dstManager;

            var w = terrainData.heightmapResolution - 1;
            var h = terrainData.heightmapResolution - 1;
            var ww = this.NumGrids.x;
            var wh = this.NumGrids.y;
            var lw = w / ww;
            var lh = h / wh;

            var unitScale = terrainData.heightmapScale.x;

            var mesh = this.UseHalfSlantMesh
                ? MeshUtility.CreateSlantHalfGridMesh(lw, lh, 1.0f)
                : MeshUtility.CreateGridMesh(lw, lh, 1.0f);
            var mat = new Material(this.DrawShader);

            initMasterEntityComponent_(entity, mat);

            var model = createModelEntity_(mesh, mat);
            createAllGrids_(this.MaxLodLevel, model, entity);

            //initEmitting_(entity);

            return;


            TerrainData getTrrainData_()
            {
                var terrain = GetComponent<Terrain>();

                if (terrain == null)
                {
                    Debug.LogError("No terrain found!");
                    return null;
                }

                return terrain.terrainData;
            }

            Entity createModelEntity_(Mesh mesh, Material mat)
            {
                mat.mainTexture = this.Texture;
                var boneLength = 1;
                var optionalVectorLength = 1;// x: grid_lv, y: grid_serial_id
                //Debug.Log(optionalVectorLength);
                //Debug.Log(mat);
                //Debug.Log(this.Texture);
                return gcs.CreateDrawModelEntityComponents(
                    this.gameObject, mesh, mat, BoneType.T, boneLength,
                    DrawModel.SortOrder.desc, optionalVectorLength);
                //return gcs.InitDrawModelEntityComponents(
                //    this.gameObject, entity, mesh, mat, BoneType.T, boneLength,
                //    DrawModel.SortOrder.desc, optionalVectorLength);
            }


            void initMasterEntityComponent_(Entity ent, Material mat)
            {

                var types = new ComponentType[]
                {
                    typeof(GridMaster.HeightFieldData),
                    typeof(GridMaster.DimensionData),
                    typeof(GridMaster.HeightFieldShaderResourceData),
                    typeof(GridMaster.InitializeTag),
                };
                em.AddComponents(ent, new ComponentTypes(types));

                var pos = (float3)this.transform.position - new float3(w, 0.0f, h) * unitScale * 0.5f;
                //em.SetComponentData(ent, new GridMaster.DimensionData
                //{
                //    NumGrids = new int2(ww, wh),
                //    UnitLengthInGrid = new int2(lw, lh),
                //    UnitScale = this.UnitDistance,
                //    LeftTopLocation = pos.As_float3().xz.x_y(),

                //    TotalLength = new int2(w, h),
                //    UnitScaleRcp = 1 / this.UnitDistance,
                //});
                //em.SetComponentData(ent, new GridMaster.HeightFieldShaderResourceData
                //{
                //    Heights = new HeightFieldBuffer()
                //});

                var dim = new GridMaster.DimensionData
                {
                    LeftTopLocation = pos,

                    TotalLength = new int2(w, h),
                    NumGrids = new int2(ww, wh),
                    UnitLengthInGrid = new int2(lw, lh),

                    UnitScale = unitScale,
                    UnitScaleRcp = 1 / unitScale,
                };
                em.SetComponentData(ent, dim);

                var res = new GridMaster.HeightFieldShaderResourceData { };
                em.SetComponentData(ent, res);

                var heights = new GridMaster.HeightFieldData { };
                heights.Alloc(dim.NumGrids, dim.UnitLengthInGrid);//
                heights.InitHeightBuffer(terrainData);
                res.SetResourcesTo(mat, dim);
            }


            void createAllGrids_(int lodlevel, Entity model, Entity area)
            {
                var q =
                    from ix in Enumerable.Range(0, ww >> lodlevel)
                    from iy in Enumerable.Range(0, wh >> lodlevel)
                    select new int2(ix, iy);
                q.ForEach(i => createGridEntity_(lodlevel, i, model, area));

                if (lodlevel - 1 < 0) return;

                createAllGrids_(lodlevel - 1, model, area);
            }


            void createGridEntity_(int lodlevel, int2 i, Entity model, Entity area)
            {
                var ent = gcs.CreateAdditionalEntity(this);

                gcs.DstEntityManager.SetName_(ent, $"heightgrid@{lodlevel}:{i.x}:{i.y}");


                var types = new List<ComponentType>
                {
                    typeof(Height.AreaLinkData),
                    typeof(Height.GridData),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),

                    typeof(Translation),
                };
                if (lodlevel == 0) types.Add(typeof(Height.GridLv0Tag));
                em.AddComponents(ent, new ComponentTypes(types.ToArray()));


                em.SetComponentData(ent, new Height.AreaLinkData
                {
                    ParentAreaEntity = area,
                });
                em.SetComponentData(ent, new Height.GridData
                {
                    GridId = i,
                    UnitScaleOnLod = unitScale * (1 << lodlevel),
                });

                em.SetComponentData(ent,
                    new DrawInstance.ModelLinkData
                    {
                        DrawModelEntityCurrent = model,
                    }
                );
                em.SetComponentData(ent,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                var total = new float3(w, 0, h) * unitScale;
                var startPosition = (float3)this.transform.position - total * 0.5f;
                var span = new float3(lw, 0, lh) * unitScale * (1 << lodlevel);
                var offset = new float3(i.x, 0, i.y) * span;
                em.SetComponentData(ent,
                    new Translation
                    {
                        Value = startPosition + offset,
                    }
                );
            }


            //void initEmitting_(Entity entity)
            //{
            //    var gcs = conversionSystem;
            //    var em = gcs.DstEntityManager;

            //    em.AddComponentData(entity, new GridMaster.Emitting
            //    {
            //        SplashPrefab = gcs.GetPrimaryEntity(this.SplashPrefab),
            //    });
            //}

        }

    }
}