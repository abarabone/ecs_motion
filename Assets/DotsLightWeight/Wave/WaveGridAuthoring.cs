using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.WaveGrid.Aurthoring
{
    using DotsLite.WaveGrid;
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Authoring;

    public class WaveGridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public float UnitDistance;
        public BinaryLength2 UnitLengthInGrid;
        public int2 NumGrids;
        public int MaxLodLevel;

        public Shader DrawShader;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;
            var em = dstManager;
            var w = (int)this.UnitLengthInGrid.u;
            var h = (int)this.UnitLengthInGrid.v;

            initMasterEntityComponent_(entity);

            var model = createModelEntity_();
            createAllGrids_(this.NumGrids, this.MaxLodLevel, model);

            return;


            Entity createModelEntity_()
            {
                var mesh = createGridMesh_();
                var mat = new Material(this.DrawShader);
                var boneLength = 1;
                var optionalVectorLength = (w * h) >> 2;
                return gcs.CreateDrawModelEntityComponents(this.gameObject, mesh, mat, BoneType.T, boneLength, optionalVectorLength);
            }

            void initMasterEntityComponent_(Entity ent)
            {
                var totalLength = w * h;
                em.AddComponentData(ent, new WaveGridMasterData
                {
                    Units = new NativeArray<WaveGridPoint>(totalLength, Allocator.Persistent),
                });
            }

            void createAllGrids_(int2 numgrids, int lodlevel, Entity model)
            {
                var q =
                    from ix in Enumerable.Range(0, numgrids.x >> lodlevel)
                    from iy in Enumerable.Range(0, numgrids.y >> lodlevel)
                    select new int2(ix, iy);
                q.ForEach(i => createGridEntity_(lodlevel, i, model));

                if (lodlevel - 1 < 0) return;

                createAllGrids_(numgrids, lodlevel - 1, model);
            }

            void createGridEntity_(int lodlevel, int2 i, Entity model)
            {
                var ent = gcs.CreateAdditionalEntity(this);

                gcs.DstEntityManager.SetName_(ent, $"wavegrid@{lodlevel}:{i.x}:{i.y}");


                var types = new ComponentTypes(new ComponentType[]
                {
                    typeof(WaveGridData),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                });
                em.AddComponents(ent, types);

                
                em.SetComponentData(ent, new WaveGridData
                {
                    gridid = i,
                    UnitScaleOnLod = this.UnitDistance * lodlevel,
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
            }

            Mesh createGridMesh_()
            {
                var mesh = new Mesh();

                var qVtxs =
                    from ix in Enumerable.Range(0, w)
                    from iz in Enumerable.Range(0, h)
                    select new Vector3(ix, 0, iz) * this.UnitDistance
                    ;
                mesh.SetVertices(qVtxs.ToArray());

                var qIdx =
                    from ix in Enumerable.Range(0, w - 1)
                    from iz in Enumerable.Range(0, h - 1)
                    let i0 = ix + (iz + 0) * w
                    let i1 = ix + (iz + 1) * w
                    from i in new int[]
                    {
                        i0+0, i0+1, i1+0,
                        i1+0, i0+1, i1+1,
                        //0, 1, 2,
                        //2, 1, 3,
                    }
                    select i
                    ;
                mesh.SetIndices(qIdx.ToArray(), MeshTopology.Triangles, 0);

                return mesh;
            }
        }

    }
}