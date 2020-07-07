using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System.Threading.Tasks;
using Unity.Linq;

namespace Abarabone.Structure.Aurthoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;


    /// <summary>
    /// 
    /// </summary>
    public class StructureModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {


        public Material Material;



        /// <summary>
        /// 
        /// </summary>
        public async void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var meshElements = await this.combinePartMeshesAsync();

            this.gameObject.AddComponent<MeshFilter>().mesh = meshElements.CreateMesh();

            return;


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

                var modelEntity_ = gcs.CreateDrawModelEntityComponents(main, mesh, mat, BoneType, boneLength);
            }

            void initInstanceEntityComponents_(GameObjectConversionSystem gcs, GameObject main)
            {
                dstManager.SetName(entity, $"{this.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var archetype = em.CreateArchetype(
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
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
                        DrawModelEntity = gcs.GetFromModelEntityDictionary(main),
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
        /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
        /// </summary>
        async Task<MeshElements> combinePartMeshesAsync()
        {

            // 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
            // （ただしパーツだった場合は、結合対象から除外する）
            var buildTargets = queryTargets_Recursive_(this.gameObject).Do(x=>Debug.Log(x.name)).ToArray();
            if (buildTargets.Length == 1) return new MeshElements { };

            var meshElements = await combineChildMeshesAsync_(buildTargets, this.transform);

            return meshElements;


            IEnumerable<GameObject> queryTargets_Recursive_(GameObject go_)
            {
                var q =
                    from child in go_.Children()
                    where child.GetComponent<StructurePartAuthoring>() == null
                    from x in queryTargets_Recursive_(child)
                    select x
                    ;
                return q.Prepend(go_);
            }

            async Task<MeshElements> combineChildMeshesAsync_(IEnumerable<GameObject> targets_, Transform tf_)
            {
                var combineElementFunc =
                    MeshCombiner.BuildNormalMeshElements(targets_, tf_, isCombineSubMeshes: true);

                return await Task.Run(combineElementFunc);
            }

            //void removeOrigineComponents_(IEnumerable<GameObject> targets_)
            //{
            //    foreach (var go in targets_)
            //    {
            //        go.DestroyComponentIfExists<MeshFilter>();
            //        go.DestroyComponentIfExists<Renderer>();
            //    }
            //}

            //void replaceOrAddComponents_CombinedMeshAndMaterials_(GameObject gameObject_, MeshElements me_)
            //{
            //    var mf = gameObject_.GetComponent<MeshFilter>().As() ?? gameObject_.AddComponent<MeshFilter>();
            //    mf.sharedMesh = me_.CreateMesh();

            //    var mr = gameObject_.GetComponent<MeshRenderer>().As() ?? gameObject_.AddComponent<MeshRenderer>();
            //    mr.materials = me_.materials;
            //}
        }

    }

}
