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

            createModelEntities_(conversionSystem, this.ShaderToDraw, this.LodOptionalMeshTops);

            var drawInstatnce = initInstanceEntityComponents_(conversionSystem, this.gameObject, this.LodOptionalMeshTops);

            conversionSystem.AddLod2ComponentToDrawInstanceEntity(drawInstatnce, this.gameObject, this.LodOptionalMeshTops);

            return;


            void createModelEntities_
                (GameObjectConversionSystem gcs, Shader shader, ObjectAndDistance[] lodOpts)
            {

                var atlasDict = gcs.GetTextureAtlasDictionary();
                var meshDict = gcs.GetMeshDictionary();

                this.QueryMeshTopObjects().PackTextureToDictionary(atlasDict);

                combineMeshToDictionary_();

                createModelEntities_();

                return;


                void combineMeshToDictionary_()
                {
                    var ofs = this.BuildMeshCombiners(meshDict, atlasDict);
                    var qMObj = ofs.Select(x => x.obj);
                    //var qMesh = ofs.Select(x => x.f.ToTask())
                    //    .WhenAll().Result
                    //    .Select(x => x.CreateMesh());
                    var qMesh = ofs.Select(x => x.f().CreateMesh());
                    meshDict.AddRange(qMObj, qMesh);
                }

                void createModelEntities_()
                {
                    var qObj = this.QueryMeshTopObjects();

                    foreach (var obj in qObj)
                    {
                        Debug.Log($"{obj.name} model ent");

                        var mesh = meshDict[obj];
                        var atlas = atlasDict.objectToAtlas[obj];
                        createModelEntity_(obj, mesh, atlas);
                    }

                    return;


                    void createModelEntity_(GameObject obj, Mesh mesh, Texture2D atlas)
                    {
                        var mat = new Material(shader);
                        mat.enableInstancing = true;
                        mat.mainTexture = atlas;
                        
                        const BoneType BoneType = BoneType.TR;
                        const int boneLength = 1;

                        gcs.CreateDrawModelEntityComponents(obj, mesh, mat, BoneType, boneLength);
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


        ///// <summary>
        ///// この GameObject をルートとしたメッシュを結合する、メッシュ生成デリゲートを列挙して返す。
        ///// ただし LodOptionalMeshTops に登録した「ＬＯＤメッシュ」のみを対象とする。
        ///// デフォルトメッシュは結合対象にはならない。
        ///// またＬＯＤに null を登録した場合は、ルートから検索して最初に発見したメッシュを
        ///// 加工せずに採用するため、この関数では配列に null を格納して返される。
        ///// 返される要素数は、 LodOptionalMeshTops.Length と同じ。
        ///// </summary>
        //public Func<MeshCombinerElements>[] GetMeshCombineFuncPerObjects()
        //{
        //    var qResult = Enumerable.Empty<Func<MeshCombinerElements>>();

        //    if (this.LodOptionalMeshTops.Length == 0) return qResult.ToArray();

        //    return this.LodOptionalMeshTops
        //        .Select(x => x.objectTop)
        //        .Select(lod => lod != null
        //           ? MeshCombiner.BuildNormalMeshElements(lod.ChildrenAndSelf(), this.transform)
        //           : null
        //        )
        //        .ToArray();
        //}

        /// <summary>
        /// この GameObject をルートとしたメッシュを結合する、メッシュ生成デリゲートを列挙して返す。
        /// ただし LodOptionalMeshTops に登録した「ＬＯＤメッシュ」があれば、そちらを対象とする。
        /// またＬＯＤに null を登録した場合は、この GameObject をルートとしたメッシュが対象となる。
        /// なお、すでに ConvertedMeshDictionary に登録されている場合も除外される。
        /// </summary>
        public override (GameObject obj, Func<IMeshElements> f)[] BuildMeshCombiners
            (Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary, Transform[] bones = null)
        {
            var objs = this.QueryMeshTopObjects()
                .Where(x => !meshDictionary.ContainsKey(x))
                .ToArray();
            var mmtss = objs
                .Select(obj => obj.QueryMeshMatsTransform_IfHaving())
                .ToArrayRecursive2();
            var qMeshData = mmtss.QueryMeshDataWithDisposingLastIn();

            var qObjAndBuilder =
                from src in (objs, mmtss, qMeshData).Zip()
                let obj = src.src0
                let mmts = src.src1
                let meshes = src.src2
                let atlas = atlasDictionary.objectToAtlas[obj].GetHashCode()
                let dict = atlasDictionary.texHashToUvRect
                select (
                    obj,
                    mmts.BuildCombiner<UI32, PositionNormalUvVertex>
                        (obj.transform, meshes, part => dict[atlas, part])
                );
            return qObjAndBuilder.ToArray();
        }


        public override IEnumerable<GameObject> QueryMeshTopObjects()
        {
            var qMain = this.gameObject.AsEnumerable()
                .Where(x => this.LodOptionalMeshTops.Length == 0);

            var qLod = this.LodOptionalMeshTops
                .Select(x => x.objectTop ?? this.gameObject);

            return qMain.Concat(qLod)
                .Distinct();
        }


        //public override void BuildMeshAndAtlasToDictionary(GameObjectConversionSystem gcs, IEnumerable<GameObject> objs)
        //{
        //    var atlasDict = gcs.GetTextureAtlasDictionary();
        //    var meshDict = gcs.GetMeshDictionary();


        //    var tex = toAtlas_();

        //    if (tex.atlas != null) combineMeshes_(tex);

        //    return;


        //    TextureAtlasAndParameter toAtlas_()
        //    {
        //        var tobjs = objs
        //            .Where(x => !atlasDict.objectToAtlas.ContainsKey(x))
        //            //.Logging(x => x.name)
        //            .ToArray();

        //        if (tobjs.Length == 0) return default;

        //        var qMat =
        //            from obj in tobjs
        //            from r in obj.GetComponentsInChildren<Renderer>()
        //            from mat in r.sharedMaterials
        //            select mat
        //            ;

        //        var tex = qMat.QueryUniqueTextures().ToAtlasAndParameter();

        //        atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
        //        atlasDict.objectToAtlas.AddRange(tobjs, tex.atlas);

        //        return tex;
        //    }

        //    void combineMeshes_(TextureAtlasAndParameter tex)
        //    {
        //        var mobjs = objs
        //            .Where(x => !meshDict.ContainsKey(x))
        //            .ToArray();
        //        var qSrc =
        //            from obj in mobjs
        //            let mmt = obj.QueryMeshMatsTransform_IfHaving()
        //            select (obj, mmt)
        //            ;
        //        var srcs = qSrc.ToArray();

        //        var qMeshSingle =
        //            from src in srcs
        //            where src.mmt.IsSingle()
        //            select src.mmt.First().mesh
        //            ;
        //        var qMeshSrc =
        //            from src in srcs
        //            where !src.mmt.IsSingle()
        //            select src.mmt.BuildCombiner<UI32, PositionNormalUvVertex>(src.obj.transform, tex).ToTask()
        //            ;
        //        var qMesh = qMeshSrc
        //            .WhenAll().Result
        //            .Select(x => x.CreateMesh())
        //            .Concat(qMeshSingle);

        //        meshDict.AddRange(mobjs, qMesh);
        //    }

        //}


        //public TextureAtlasAndParameter ToAtlas
        //    (TextureAtlasDictionary.Data atlasDict, IEnumerable<GameObject> objs)
        //{
        //    var texobjs = objs
        //        .Where(x => !atlasDict.objectToAtlas.ContainsKey(x))
        //        //.Logging(x => x.name)
        //        .ToArray();

        //    if (texobjs.Length == 0) return default;

        //    var qMat =
        //        from obj in texobjs
        //        from r in obj.GetComponentsInChildren<Renderer>()
        //        from mat in r.sharedMaterials
        //        select mat
        //        ;

        //    var tex = qMat.QueryUniqueTextures().ToAtlasAndParameter();

        //    atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
        //    atlasDict.objectToAtlas.AddRange(texobjs, tex.atlas);

        //    return tex;
        //}

        //void CombineMesh(TextureAtlasAndParameter tex)
        //{
        //    var mobjs = this.QueryMeshTopObjects()
        //        .Where(x => !meshDict.ContainsKey(x))
        //        .ToArray();
        //    var qSrc =
        //        from obj in mobjs
        //        let mmt = obj.QueryMeshMatsTransform_IfHaving()
        //        select (obj, mmt)
        //        ;
        //    var srcs = qSrc.ToArray();

        //    var qMeshSingle =
        //        from src in srcs
        //        where src.mmt.IsSingle()
        //        select src.mmt.First().mesh
        //        ;
        //    var qMeshSrc =
        //        from src in srcs
        //        where !src.mmt.IsSingle()
        //        select src.mmt.BuildCombiner<UI32, PositionNormalUvVertex>(src.obj.transform, tex).ToTask()
        //        ;
        //    var qMesh = qMeshSrc
        //        .WhenAll().Result
        //        .Select(x => x.CreateMesh())
        //        .Concat(qMeshSingle);

        //    meshDict.AddRange(mobjs, qMesh);
        //}

    }

}
