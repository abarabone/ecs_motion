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

    public class WaveGridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public float UnitDistance;
        public BinaryLength2 UnitLengthInGrid;
        public BinaryLength2 NumGrids;
        public int MaxLodLevel;

        public Shader DrawShader;
        public Texture Texture;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;
            var em = dstManager;

            var ww = (int)this.NumGrids.u;
            var wh = (int)this.NumGrids.v;
            var lw = (int)this.UnitLengthInGrid.u;
            var lh = (int)this.UnitLengthInGrid.v;


            initMasterEntityComponent_(entity);

            var model = createModelEntity_();
            createAllGrids_(this.MaxLodLevel, model);

            return;


            Entity createModelEntity_()
            {
                var mesh = MeshUtility.CreateGridMesh(lw, lh, this.UnitDistance);
                var mat = new Material(this.DrawShader);
                mat.mainTexture = this.Texture;
                var boneLength = 1;
                var optionalVectorLength = (((lw + 1) * (lh + 1)) >> 2) + 1;Debug.Log(optionalVectorLength);
                return gcs.CreateDrawModelEntityComponents(this.gameObject, mesh, mat, BoneType.T, boneLength, optionalVectorLength);
            }


            void initMasterEntityComponent_(Entity ent)
            {
                var totalLength = ww*lw * wh*lh + wh*lh;// 最後に１ライン余分に加え、ループ用にコピーエリアとする
                em.AddComponentData(ent, new Wave.GridMasterData
                {
                    Prevs = new NativeArray<float>(totalLength, Allocator.Persistent),
                    Currs = new NativeArray<float>(totalLength, Allocator.Persistent),
                    Nexts = new NativeArray<float>(totalLength, Allocator.Persistent),
                    NumGrids = this.NumGrids,
                    UnitLengthInGrid = this.UnitLengthInGrid,
                    UnitScale = this.UnitDistance,
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

        }

    }
}