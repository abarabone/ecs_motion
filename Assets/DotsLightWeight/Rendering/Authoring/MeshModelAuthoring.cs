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
        public Shader ShaderToDraw;

        [SerializeField]
        public ObjectAndDistance[] LodOptionalMeshTops;


        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createModelEntities_(conversionSystem, this.gameObject, this.ShaderToDraw, this.LodOptionalMeshTops);

            var drawInstatnce = initInstanceEntityComponents_(conversionSystem, this.gameObject, this.LodOptionalMeshTops);

            conversionSystem.AddLod2ComponentToDrawInstanceEntity(drawInstatnce, this.gameObject, this.LodOptionalMeshTops);

            return;


            void createModelEntities_
                (GameObjectConversionSystem gcs, GameObject top, Shader shader, ObjectAndDistance[] lodOpts)
            {

                var atlasDict = gcs.GetTextureAtlasDictionary();
                var meshDict = gcs.GetMeshDictionary();

                var objs = queryMeshTopObjects().ToArray();

                var tex = toAtlases_(objs);
                combineMeshes_();
                createModelEntities_();

                return;




                var q =
                    from obj in objs
                    select atlasDict.objectToAtlas.ContainsKey(obj) switch
                    {
                        true =>
                            atlasDict.objectToAtlas[obj],
                        false =>
                            
                    };

                TextureAtlasAndParameter toAtlases_(IEnumerable<GameObject> objects)
                {
                    if (atlasDict.objectToAtlas.ContainsKey(top)) return default;

                    var qMat =
                        from r in this.GetComponentsInChildren<Renderer>()
                        from mat in r.sharedMaterials
                        select mat
                        ;
                    var tex = qMat.QueryUniqueTextures().ToAtlasAndParameter();

                    atlasDict.objectToAtlas[top] = tex.atlas;
                    atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;

                    return tex;
                }

                void combineMeshes_()
                {
                    var combiner = this.BuildMeshCombiners<UI32, PositionNormalUvVertex>(meshDict, tex);
                    var qMesh = combiner.fs
                        .Select(f => f.ToTask())
                        .WhenAll().Result
                        .Select(m => m.CreateMesh());
                    foreach (var (obj, mesh) in (combiner.objs, qMesh).Zip())
                    {
                        meshDict[obj] = mesh;
                    }
                }

                void createModelEntities_()
                {
                    var qMain = top.AsEnumerable()
                        .Where(_ => this.LodOptionalMeshTops.Length == 0);
                    var qLod = this.LodOptionalMeshTops
                        .Select(x => x.objectTop ?? top);

                    var objs = qLod.Concat(qMain)
                        .ToArray();
                    var meshes = objs
                        .Distinct()
                        .Select(obj => meshDict[obj])
                        .ToArray();

                    foreach (var (obj, mesh) in (objs, meshes).Zip())
                    {
                        Debug.Log($"{obj.name} model ent");

                        createModelEntity_(obj, mesh, tex);
                    }

                    return;


                    void createModelEntity_(GameObject obj, Mesh mesh_, TextureAtlasAndParameter tex)
                    {
                        var mat = new Material(shader);
                        mat.mainTexture = atlasDict.objectToAtlas[obj];
                        
                        const BoneType BoneType = BoneType.TR;
                        const int boneLength = 1;

                        gcs.CreateDrawModelEntityComponents(obj, mesh_, mat, BoneType, boneLength);
                    }
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
                        DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(main),//gcs.GetDrawModelEntity(main, lodOpts),
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
        public override (GameObject[] objs, Func<MeshElements<TIdx, TVtx>>[] fs) BuildMeshCombiners<TIdx, TVtx>
            (Dictionary<GameObject, Mesh> meshDictionary = null, TextureAtlasAndParameter tex = default)
        {
            var objs = queryMeshTopObjects()
                .Where(x => !(meshDictionary?.ContainsKey(x) ?? false))
                .ToArray();
            var fs = objs
                .Select(obj => obj.BuildCombiner<TIdx, TVtx>(obj.transform, tex))
                .ToArray();
            return (objs, fs);
        }

        IEnumerable<GameObject> queryMeshTopObjects()
        {
            var qMain = this.gameObject.AsEnumerable()
                .Where(x => this.LodOptionalMeshTops.Length == 0);
            var qLod = this.LodOptionalMeshTops
                .Select(x => x.objectTop ?? this.gameObject);
            return qMain.Concat(qLod)
                .Distinct();
        }


        public void BuildMeshAndAtlasToDictionary()
        {

        }
    }

}
