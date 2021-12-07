using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

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

    public class HeightGridWithAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public float UnitDistance;
        public GridBinaryLength2 UnitLengthInGrid;
        public BinaryLength2 NumGrids;
        //public int2 NumGrids;
        public int MaxLodLevel;

        public Shader DrawShader;
        public Texture Texture;

        public float Dumping = 0.999f;
        public float Constraint2 = 0.8f;

        public bool UseHalfSlantMesh;


        public ParticleAuthoringBase SplashPrefab;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            referencedPrefabs.Add(this.SplashPrefab.gameObject);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.gameObject.activeSelf) return;

            var gcs = conversionSystem;
            var em = dstManager;

            var ww = this.NumGrids.x;
            var wh = this.NumGrids.y;
            var lw = this.UnitLengthInGrid.x;
            var lh = this.UnitLengthInGrid.y;

            var mesh = this.UseHalfSlantMesh
                ? MeshUtility.CreateSlantHalfGridMesh(lw, lh, 1.0f)
                : MeshUtility.CreateGridMesh(lw, lh, 1.0f);

            initMasterEntityComponent_(entity);

            var model = createModelEntity_(mesh);
            createAllGrids_(this.MaxLodLevel, model);

            initEmitting_(entity);

            return;


            Entity createModelEntity_(Mesh mesh)
            {
                var mat = new Material(this.DrawShader);
                mat.mainTexture = this.Texture;
                var boneLength = 1;
                var optionalVectorLength = (((lw + 1) * (lh + 1)) >> 2) + 1;
                Debug.Log(optionalVectorLength);
                Debug.Log(mat);
                Debug.Log(this.Texture);
                return gcs.CreateDrawModelEntityComponents(
                    this.gameObject, mesh, mat, BoneType.T, boneLength,
                    DrawModel.SortOrder.desc, optionalVectorLength);
                //return gcs.InitDrawModelEntityComponents(
                //    this.gameObject, entity, mesh, mat, BoneType.T, boneLength,
                //    DrawModel.SortOrder.desc, optionalVectorLength);
            }


            void initMasterEntityComponent_(Entity ent)
            {

                var totalLength = ww*lw * wh*lh + wh*lh;// 最後に１ライン余分に加え、ループ用にコピーエリアとする
                var pos = this.transform.position - new Vector3(ww * lw, 0.0f, wh * lh) * this.UnitDistance * 0.5f;

                em.AddComponentData(ent, new GridMaster.DimensionData
                {
                    NumGrids = this.NumGrids,
                    UnitLengthInGrid = this.UnitLengthInGrid,
                    UnitScale = this.UnitDistance,
                    Dumping = this.Dumping,
                    Constraint2 = this.Constraint2,
                    LeftTopLocation = pos.As_float3().xz.x_y(),

                    TotalLength = this.NumGrids * (int2)this.UnitLengthInGrid,
                    UnitScaleRcp = 1 / this.UnitDistance,
                });
            }


            void createAllGrids_(int lodlevel, Entity model)
            {
                var q =
                    from ix in Enumerable.Range(0, ww >> lodlevel)
                    from iy in Enumerable.Range(0, wh >> lodlevel)
                    select new int2(ix, iy);
                q.ForEach(i => createGridEntity_(lodlevel, i, model));

                if (lodlevel - 1 < 0) return;

                createAllGrids_(lodlevel - 1, model);
            }


            void createGridEntity_(int lodlevel, int2 i, Entity model)
            {
                var ent = gcs.CreateAdditionalEntity(this);

                gcs.DstEntityManager.SetName_(ent, $"wavegrid@{lodlevel}:{i.x}:{i.y}");


                var types = new List<ComponentType>
                {
                    typeof(Height.GridData),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),

                    typeof(Translation),
                };
                if (lodlevel == 0) types.Add(typeof(Height.GridLevel0Tag));
                em.AddComponents(ent, new ComponentTypes(types.ToArray()));


                em.SetComponentData(ent, new Height.GridData
                {
                    GridId = i,
                    UnitScaleOnLod = this.UnitDistance * (1 << lodlevel),
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

                var total = this.UnitDistance * (float2)((int2)this.UnitLengthInGrid * (int2)this.NumGrids);
                var span = (float2)(int2)this.UnitLengthInGrid * this.UnitDistance * (1 << lodlevel);
                var startPosition = this.transform.position.As_float3().xz - (float2)total * 0.5f;
                var offset = (float2)i * span;
                em.SetComponentData(ent,
                    new Translation
                    {
                        Value = (startPosition + offset).x_y(),
                    }
                );
            }


            void initEmitting_(Entity entity)
            {
                var gcs = conversionSystem;
                var em = gcs.DstEntityManager;

                em.AddComponentData(entity, new GridMaster.Emitting
                {
                    SplashPrefab = gcs.GetPrimaryEntity(this.SplashPrefab),
                });
            }

        }

    }
}