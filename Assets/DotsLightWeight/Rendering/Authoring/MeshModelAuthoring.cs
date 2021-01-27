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
    using Abarabone.Structure.Authoring;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Misc;

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

            conversionSystem.AddLod2ComponentToDrawInstanceEntity(drawInstatnce, this.gameObject, this.LodOptionalMeshTops);

            return;


            void createModelEntities_
                (GameObjectConversionSystem gcs, GameObject top, Material srcMaterial, ObjectAndDistance[] lodOpts)
            {

                var meshDict = gcs.GetMeshDictionary();

                var combiner = this.BuildMeshCombiners<UI32, PositionUvVertex>(meshDict);
                var qMesh = combiner.fs
                    .Select(f => f.ToTask())
                    .WhenAll().Result
                    .Select(m => m.CreateMesh());
                foreach(var (obj, mesh) in (combiner.objs, qMesh).Zip())
                {
                    gcs.AddToMeshDictionary(obj, mesh);
                }
                   

                var lods = LodOptionalMeshTops
                    .Select(x => x.objectTop)
                    .ToArray();
                var main = this.gameObject.AsEnumerable()
                    .Where(_ => lods.Length == 0)
                    .ToArray();
                var meshes = lods.Concat(main)
                    .Select(obj => gcs.GetFromMeshDictionary(obj))
                    .ToArray();


                foreach(var (obj, mesh) in (lods.Concat(main), meshes).Zip())
                {
                    Debug.Log($"{obj.name} model ent");

                    createModelEntity_(obj, mesh);
                }

                return;


                void createModelEntity_(GameObject go, Mesh mesh_)
                {
                    var mat = new Material(srcMaterial);

                    const BoneType BoneType = BoneType.TR;
                    const int boneLength = 1;

                    gcs.CreateDrawModelEntityComponents(go, mesh_, mat, BoneType, boneLength);
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
        /// デフォルトメッシュは結合対象にはならない。
        /// またＬＯＤに null を登録した場合は、ルートから検索して最初に発見したメッシュを
        /// 加工せずに採用するため、この関数では配列に null を格納して返される。
        /// 返される要素数は、 LodOptionalMeshTops.Length と同じ。
        /// </summary>
        public Func<MeshCombinerElements>[] GetMeshCombineFuncPerObjects()
        {
            var qResult = Enumerable.Empty<Func<MeshCombinerElements>>();

            if (this.LodOptionalMeshTops.Length == 0) return qResult.ToArray();

            return this.LodOptionalMeshTops
                .Select(x => x.objectTop)
                .Select(lod => lod != null
                   ? MeshCombiner.BuildNormalMeshElements(lod.ChildrenAndSelf(), this.transform)
                   : null
                )
                .ToArray();
        }

        /// <summary>
        /// この GameObject をルートとしたメッシュを結合する、メッシュ生成デリゲートを列挙して返す。
        /// ただし LodOptionalMeshTops に登録した「ＬＯＤメッシュ」があれば、そちらを対象とする。
        /// またＬＯＤに null を登録した場合は、この GameObject をルートとしたメッシュが対象となる。
        /// なお、すでに ConvertedMeshDictionary に登録されている場合も除外される。
        /// </summary>
        public (GameObject[] objs, Func<MeshElements<TIdx, TVtx>>[] fs) BuildMeshCombiners<TIdx, TVtx>
            (Dictionary<GameObject, Mesh> meshDictionary = null, TextureAtlasParameter tex = default)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        {
            if (this.LodOptionalMeshTops.Length == 0)
            {
                if (meshDictionary?.ContainsKey(this.gameObject) ?? false)
                    return (new GameObject[0], new Func<MeshElements<TIdx, TVtx>>[0]);

                var f = this.gameObject.BuildCombiner<TIdx, TVtx>(this.transform, tex);

                return (this.gameObject.AsEnumerable().ToArray(), f.AsEnumerable().ToArray());
            }

            var objs = this.LodOptionalMeshTops
                .Select(x => x.objectTop ?? this.gameObject)
                .Distinct()
                .Where(x => !(meshDictionary?.ContainsKey(x) ?? false))
                .ToArray();
            var fs = objs
                .Select(obj => obj.BuildCombiner<TIdx, TVtx>(obj.transform, tex))
                .ToArray();
            return (objs, fs);
        }
    }

}
