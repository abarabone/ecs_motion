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

    public class WaveGridAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public float UnitDistance;
        public int2 UnitLengthInGrid;
        public int2 NumGrids;

        public Shader DrawShader;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var gcs = conversionSystem;
            var em = dstManager;

            var mesh = createGridMesh();
            var mat = new Material(this.DrawShader);
            gcs.CreateDrawModelEntityComponents(this.gameObject, mesh, mat, BoneType.T, 1, 
                
                );

            setMasterComponent();
            gcs.CreateDrawModelEntityComponents(this.gameObject, mesh, mat, BoneType.)
            createAllGrids_(this.NumGrids, lodlevel: 0, model);

            return;


            void setMasterComponent()
            {
                var totalLength = this.UnitLengthInGrid.x * this.UnitLengthInGrid.y;
                em.AddComponentData(entity, new WaveGridMasterData
                {
                    Units = new NativeArray<WaveUnit>(totalLength, Allocator.Persistent),
                });
            }

            void createAllGrids_(int2 numgrids, int lodlevel, Entity model)
            {
                var q =
                    from ix in Enumerable.Range(0, numgrids.x >> lodlevel)
                    from iy in Enumerable.Range(0, numgrids.y >> lodlevel)
                    select new int2(ix, iy);
                q.ForEach(i => createGridEntity_(lodlevel, i, model));
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

            Mesh createGridMesh()
            {

            }
        }

    }
}
namespace DotsLite.WaveGrid
{
    public class WaveGridMasterData : IComponentData, IDisposable
    {
        public NativeArray<WaveUnit> Units;
        public int2 UnitLengthInGrid;
        public int2 NumGrids;

        public void Dispose()
        {
            this.Units.Dispose();
        }
    }
    public struct WaveUnit
    {
        public float Next;
        public float Curr;
        public float Prev;
    }

    public struct WaveGridData : IComponentData
    {
        public int2 gridid;
        public float UnitScaleOnLod;
    }
}