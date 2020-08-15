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
using Unity.Linq;

namespace Abarabone.Particle.Aurthoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;

    /// <summary>
    /// 
    /// </summary>
    public class MeshModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {


        public Material Material;

        [SerializeField]
        public ObjectAndDistance[] LodOptionalMeshTops;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createModelEntities_(conversionSystem, this.gameObject, this.Material, this.LodOptionalMeshTops);

            var drawInstatnce = initInstanceEntityComponents_(conversionSystem, this.gameObject, this.LodOptionalMeshTops);

            conversionSystem.AddLodComponentToDrawInstanceEntity(drawInstatnce, this.gameObject, this.LodOptionalMeshTops);

            return;


            void createModelEntities_
                (GameObjectConversionSystem gcs, GameObject main, Material srcMaterial, ObjectAndDistance[] lodOpts)
            {

                var lods = LodOptionalMeshTops.Select(x => x.objectTop).ToArray();
                var meshes = gcs.GetMeshesToCreateModelEntity(main, lods, this.GetMeshCombineFuncs);

                foreach(var mesh in meshes)
                {
                    createModelEntity_(gcs, main, srcMaterial, mesh);
                }

                return;


                void createModelEntity_
                    (GameObjectConversionSystem gcs_, GameObject main_, Material srcMaterial_, Mesh mesh_)
                {
                    var mat = new Material(srcMaterial_);

                    const BoneType BoneType = BoneType.TR;
                    const int boneLength = 1;

                    gcs_.CreateDrawModelEntityComponents(main_, mesh_, mat, BoneType, boneLength);
                }

            }

            Entity initInstanceEntityComponents_(GameObjectConversionSystem gcs, GameObject main, ObjectAndDistance[] lodOpts)
            {
                dstManager.SetName_(entity, $"{this.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var archetype = em.CreateArchetype(
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(BinderTrimBlankLinkedEntityGroupTag),
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
                        DrawModelEntityCurrent = gcs.GetDrawModelEntity(main, lodOpts),
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
                );

                return mainEntity;
            }

        }



        /// <summary>
        /// この GameObject をルートとしたメッシュを結合する、メッシュ生成デリゲートを列挙して返す。
        /// ただし LodOptionalMeshTops に登録した「ＬＯＤメッシュ」のみを対象とする。
        /// ＬＯＤに未登録の場合は、ルートから検索して最初に発見したメッシュを、加工せずに採用するため、
        /// この関数では返さない。
        /// とりあえず現状はＬＯＤ２つまで。
        /// </summary>
        public Func<MeshElements>[] GetMeshCombineFuncs()
        {
            var qResult = Enumerable.Empty<Func<MeshElements>>();


            var lods = this.LodOptionalMeshTops
                .Select(x => x.objectTop)
                .Where(x => x != null)
                .ToArray();

            if (lods.Length == 0) return qResult.ToArray();


            var combineFunc0 = (lods.Length >= 1)
                ? MeshCombiner.BuildNormalMeshElements(lods[0].ChildrenAndSelf(), lods[0].transform)
                : null;

            var combineFunc1 = (lods.Length >= 2)
                ? MeshCombiner.BuildNormalMeshElements(lods[1].ChildrenAndSelf(), lods[1].transform)
                : null;

            return qResult
                .Append(combineFunc0)
                .Append(combineFunc1)
                .Where(x => x != null)
                .ToArray();
        }
    }

}
