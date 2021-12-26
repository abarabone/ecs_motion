using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;

using Material = UnityEngine.Material;

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
            Debug.Log($"{w} {h} / {ww} {wh} / {lw} {lh}");

            var unitScale = terrainData.heightmapScale.x;
            Debug.Log(unitScale);


            var mesh = this.UseHalfSlantMesh
                ? MeshUtility.CreateSlantHalfGridMesh(lw, lh, 1.0f)
                : MeshUtility.CreateGridMesh(lw, lh, 1.0f);
            var mat = new Material(this.DrawShader);
            var model = createModelEntity_(mesh, mat);

            var (heights, dim, res) = initMasterEntityComponent_(entity, mat);

            var filter = new CollisionFilter
            {
                BelongsTo = this.belongsTo.Value,
                CollidesWith = this.collidesWith.Value,
                GroupIndex = 0,
            };
            createAllGrids_(this.MaxLodLevel, model, entity, filter);
                //.ForEach(_ => { });

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


            (GridMaster.HeightFieldData, GridMaster.DimensionData, GridMaster.HeightFieldShaderResourceData)
                initMasterEntityComponent_(Entity ent, Material mat)
            {

                var types = new ComponentType[]
                {
                    typeof(GridMaster.HeightFieldData),
                    typeof(GridMaster.DimensionData),
                    typeof(GridMaster.HeightFieldShaderResourceData),
                    typeof(GridMaster.InitializeTag),

                    //typeof(PhysicsCollider),
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
                res.Alloc(new int2(ww, wh), new int2(lw, lh));

                var heights = new GridMaster.HeightFieldData { };
                heights.Alloc(dim.NumGrids, dim.UnitLengthInGrid);
                heights.InitHeightBuffer(terrainData);
                Debug.Log(dim.UnitScale);
                res.SetResourcesTo(mat, dim);

                return (heights, dim, res);
            }


            //IEnumerable<Entity> createAllGrids_(int lodlevel, Entity model, Entity area)
            //{
            //    var qEntity =
            //        from iy in Enumerable.Range(0, wh >> lodlevel)
            //        from ix in Enumerable.Range(0, ww >> lodlevel)
            //        let i = new int2(ix, iy)
            //        select createGridEntity_(lodlevel, i, model, area)
            //        ;

            //    if (lodlevel - 1 < 0) return qEntity;

            //    return createAllGrids_(lodlevel - 1, model, area)
            //        .Concat(qEntity);
            //}
            void createAllGrids_(int lodlevel, Entity model, Entity area, CollisionFilter filter)
            {
                var q =
                    from iy in Enumerable.Range(0, wh >> lodlevel)
                    from ix in Enumerable.Range(0, ww >> lodlevel)
                    select new int2(ix, iy)
                    ;
                q.ForEach(i => createGridEntity_(lodlevel, i, model, area, filter));

                if (lodlevel - 1 < 0) return;

                createAllGrids_(lodlevel - 1, model, area, filter);
            }


            Entity createGridEntity_(int lodlevel, int2 i, Entity model, Entity area, CollisionFilter filter)
            {
                var ent = gcs.CreateAdditionalEntity(this);

                gcs.DstEntityManager.SetName_(ent, $"heightgrid@{lodlevel}:{i.x}:{i.y}");


                var types = new List<ComponentType>
                {
                    typeof(HeightGrid.AreaLinkData),
                    typeof(HeightGrid.GridData),
                    typeof(HeightGrid.BlockBufferOnGpuData),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),

                    typeof(PhysicsCollider),
                    typeof(Translation),
                };
                if (lodlevel == 0) types.Add(typeof(HeightGrid.GridLv0Tag));
                em.AddComponents(ent, new ComponentTypes(types.ToArray()));


                em.SetComponentData(ent, new HeightGrid.AreaLinkData
                {
                    ParentAreaEntity = area,
                });
                em.SetComponentData(ent, new HeightGrid.GridData
                {
                    GridId = i,
                    SerialIndex = srcSerialIndex_(i.x, i.y),
                    UnitScaleOnLod = unitScale * (1 << lodlevel),
                    LodLevel = lodlevel,
                });
                em.SetComponentData(ent, new HeightGrid.BlockBufferOnGpuData
                {
                    SerialIndex = dstSerialIndex_(i.x, i.y),
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

                using var buffer = heights.MakeCopyOfGridBuffer(dim, srcSerialIndex_(i.x, i.y), 0, new int2(lw, lh));
                buffer.CopyToResource(res, dim, dstSerialIndex_(i.x, i.y));
                var collider = buffer.CreateCollider(dim, filter);
                em.SetComponentData(ent, new PhysicsCollider
                {
                    Value = collider,
                });

                return ent;
            }

            int srcSerialIndex_(int x, int y) => y * lh * w + x * lw;

            int dstSerialIndex_(int x, int y)
            {
                var gridSerialId = y * ww + x;
                var gridBlockSpan = (lh + 1) * (lw + 1);
                return gridSerialId * gridBlockSpan;
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