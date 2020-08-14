using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Particle.Aurthoring
{
    using Model;
    using Draw;
    using Model.Authoring;
    using Draw.Authoring;
    using Abarabone.Geometry;
    using Unity.Linq;
    using Abarabone.Structure.Authoring;

    /// <summary>
    /// 
    /// </summary>
    public class MeshModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {


        public Material Material;

        [SerializeField]
        public ObjectAndDistance[] LodOptionalMeshTops;
        [Serializable]
        public class ObjectAndDistance
        {
            public GameObject objectTop;
            public float distance;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createModelEntity_(conversionSystem, this.gameObject, this.Material);

            initInstanceEntityComponents_(conversionSystem, this.gameObject);

            return;


            void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject main, Material srcMaterial)
            {
                var mat = new Material(srcMaterial);
                var mesh = main.GetComponentInChildren<MeshFilter>().sharedMesh;

                const BoneType BoneType = BoneType.TR;
                const int boneLength = 1;


                if (this.LodOptionalMeshTops.Length >= 1)
                {
                    var mesh = gcs.GetFromStructureMeshDictionary(this.LodOptionalMeshTops[0].objectTop);
                    if(mesh == null) mesh = 
                }
                gcs.CreateDrawModelEntityComponents(main, mesh, mat, BoneType, boneLength);
                
            }

            void initInstanceEntityComponents_(GameObjectConversionSystem gcs, GameObject main)
            {
                dstManager.SetName_(entity, $"{this.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var archetype = em.CreateArchetype(
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(DrawInstance.MeshTag),
                    typeof(DrawInstance.ModeLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(Translation),
                    typeof(Rotation),
                    typeof(NonUniformScale)
                );
                em.SetArchetype(mainEntity, archetype);


                em.SetComponentData(mainEntity,
                    new DrawInstance.ModeLinkData
                    //new DrawTransform.LinkData
                    {
                        DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(main),
                    }
                );
                em.SetComponentData(mainEntity,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                em.SetComponentData(mainEntity,
                    new Translation
                    {
                        Value = float3.zero,
                    }
                );
                em.SetComponentData(mainEntity,
                    new Rotation
                    {
                        Value = quaternion.identity,
                    }
                );
                em.SetComponentData(mainEntity,
                    new NonUniformScale
                    {
                        Value = new float3(1.0f, 1.0f, 1.0f),
                    }
                ); ;
            }

        }


        /// <summary>
        /// この GameObject をルートとしたメッシュを結合する、メッシュ生成デリゲートを列挙して返す。
        /// ただし LodOptionalMeshTops に登録した「ＬＯＤメッシュ」のみを対象とする。
        /// ＬＯＤに未登録の場合は、ルートから検索して最初に発見したメッシュを、加工せずに採用するため、
        /// この関数では返さない。
        /// とりあえず現状はＬＯＤ２つまで。
        /// </summary>
        public IEnumerable<Func<MeshElements>> GetMeshCombineFuncs()
        {
            var qResult = Enumerable.Empty<Func<MeshElements>>();


            var lods = this.LodOptionalMeshTops
                .Where(x => x != null)
                .Select(x => x.objectTop)
                .ToArray();

            if (lods.Length == 0) return qResult;


            var combineFunc0 = (lods.Length >= 1)
                ? MeshCombiner.BuildNormalMeshElements(lods[0].ChildrenAndSelf(), lods[0].transform)
                : null;

            var combineFunc1 = (lods.Length >= 2)
                ? MeshCombiner.BuildNormalMeshElements(lods[1].ChildrenAndSelf(), lods[1].transform)
                : null;

            return qResult
                .Append(combineFunc0)
                .Append(combineFunc1)
                .Where(x => x != null);
        }
    }

}
