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

                this.OmmtsEnumerable.Objs().PackTextureToDictionary(atlasDict);

                combineMeshToDictionary_();

                createModelEntities_();

                return;


                void combineMeshToDictionary_()
                {
                    using var meshAll = this.OmmtsEnumerable.QueryMeshDataWithDisposingLast();

                    var ofs = this.BuildMeshCombiners(meshAll.AsEnumerable, meshDict, atlasDict);
                    var qMObj = ofs.Select(x => x.obj);
                    var qMesh = ofs.Select(x => x.f.ToTask())
                        .WhenAll().Result
                        .Select(x => x.CreateMesh());
                    //var qMesh = ofs.Select(x => x.f().CreateMesh());
                    meshDict.AddRange(qMObj, qMesh);
                }

                void createModelEntities_()
                {
                    var qObj = this.OmmtsEnumerable.Objs();

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


        /// <summary>
        /// この GameObject をルートとしたメッシュを結合する、メッシュ生成デリゲートを列挙して返す。
        /// ただし LodOptionalMeshTops に登録した「ＬＯＤメッシュ」があれば、そちらを対象とする。
        /// またＬＯＤに null を登録した場合は、この GameObject をルートとしたメッシュが対象となる。
        /// なお、すでに ConvertedMeshDictionary に登録されている場合も除外される。
        /// </summary>
        public override (GameObject obj, Func<IMeshElements> f)[] BuildMeshCombiners
            (
                IEnumerable<SrcMeshCombinePack> meshpacks,
                Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
            )
        {
            var qObjAndBuilder =
                from src in meshpacks
                let atlas = atlasDictionary.objectToAtlas[src.obj].GetHashCode()
                let texdict = atlasDictionary.texHashToUvRect
                where !meshDictionary.ContainsKey(src.obj)
                select (
                    src.obj,
                    src.BuildCombiner<UI32, PositionNormalUvVertex>(part => texdict[atlas, part])
                );
            return qObjAndBuilder.ToArray();
        }



        IEnumerable<ObjectAndMmts> _ommtss = null;

        public override IEnumerable<ObjectAndMmts> OmmtsEnumerable => this._ommtss ??=
            from obj in this.combineTargetObjects().ToArray()
            select new ObjectAndMmts
            {
                obj = obj,
                mmts = obj.QueryMeshMatsTransform_IfHaving().ToArray(),
            };

        IEnumerable<GameObject> combineTargetObjects()
        {
            var qMain = this.gameObject.WrapEnumerable()
                .Where(x => this.LodOptionalMeshTops.Length == 0);

            var qLod = this.LodOptionalMeshTops
                .Select(x => x.objectTop ?? this.gameObject);

            return qMain.Concat(qLod)
                .Distinct();
        }

    }

}
