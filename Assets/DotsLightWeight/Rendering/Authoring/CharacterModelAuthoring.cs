using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Model.Authoring
{
    using Abarabone.Draw.Authoring;
    using Abarabone.Character;
    using Abarabone.Common.Extension;
    using Abarabone.CharacterMotion.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Utilities;

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class CharacterModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public Shader DrawShader;


        public EnBoneType BoneMode;



        /// <summary>
        /// 描画関係はバインダーに、ボーン関係はメインに関連付ける
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject;
            var main = top.Children().First();

            createModelEntity_(conversionSystem, this.DrawShader, this.bones);

            initBinderEntity_(conversionSystem, top, main);
            initMainEntity_(conversionSystem, top, main);

            conversionSystem.InitPostureEntity(main);//, bones);
            conversionSystem.InitBoneEntities(main, this.bones, main.transform, this.BoneMode);

            conversionSystem.CreateDrawInstanceEntities(top, main, this.bones, this.BoneMode);

            return;



            void createModelEntity_(GameObjectConversionSystem gcs, Shader shader, Transform[] bones)
            {

                var atlasDict = gcs.GetTextureAtlasDictionary();
                var meshDict = gcs.GetMeshDictionary();

                this.OmmtsEnumerable.Objs().PackTextureToDictionary(atlasDict);

                combineMeshToDictionary_();

                createModelEntities_();

                return;


                void combineMeshToDictionary_()
                {
                    using var meshAll = this.OmmtsEnumerable.QueryMeshDataFromModel();

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
                    var objs = this.OmmtsEnumerable.Objs();

                    foreach (var obj in objs)
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

                        gcs.CreateDrawModelEntityComponents(top, mesh, mat, BoneType, bones.Length);
                    }
                }

            }








            static void initBinderEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(main_);


                var binderAddtypes = new ComponentTypes
                (
                    typeof(BinderTrimBlankLinkedEntityGroupTag),
                    typeof(ObjectBinder.MainEntityLinkData)
                );
                em_.AddComponents(binderEntity, binderAddtypes);

                em_.SetComponentData(binderEntity,
                    new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


                em_.SetName_(binderEntity, $"{top_.name} binder");
            }

            static void initMainEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(main_);


                var mainAddtypes = new ComponentTypes
                (
                    typeof(ObjectMain.ObjectMainTag),
                    typeof(ObjectMain.BinderLinkData),
                    typeof(ObjectMainCharacterLinkData)
                //typeof(ObjectMain.MotionLinkDate)
                );
                em_.AddComponents(mainEntity, mainAddtypes);

                em_.SetComponentData(mainEntity,
                    new ObjectMain.BinderLinkData
                    {
                        BinderEntity = binderEntity,
                    }
                );
                em_.SetComponentData(mainEntity,
                    new ObjectMainCharacterLinkData
                    {
                        PostureEntity = mainEntity,//
                    }
                );


                em_.SetName_(mainEntity, $"{top_.name} main");
            }

        }





        
        public override (GameObject obj, Func<IMeshElements> f)[] BuildMeshCombiners
            (
                IEnumerable<SrcMeshesModelCombinePack> meshpacks,
                Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
            )
        {
            var qObjAndBuilder =
                from src in meshpacks
                where !meshDictionary.ContainsKey(src.obj)
                let atlas = atlasDictionary.objectToAtlas[src.obj].GetHashCode()
                let texdict = atlasDictionary.texHashToUvRect
                select (
                    src.obj,
                    src.BuildCombiner<UI32, PositionNormalUvBonedVertex>
                        (part => texdict[atlas, part], this.bones)
                );
            return qObjAndBuilder.ToArray();
        }




        IEnumerable<ObjectAndMmts> _ommtss = null;

        public override IEnumerable<ObjectAndMmts> OmmtsEnumerable =>  this._ommtss ??=
            from obj in this.combineTargetObjects().ToArray()
            select new ObjectAndMmts
            {
                obj = obj,
                mmts = obj.QueryMeshMatsTransform_IfHaving().ToArray(),
            };

        IEnumerable<GameObject> combineTargetObjects()
        {
            return this.gameObject.WrapEnumerable();// 後でLODに対応させよう、とりあえずは単体で
        }



        Transform[] _bones = null;

        Transform[] bones => this._bones ??=
            this.queryBones().ToArray();
        
        IEnumerable<Transform> queryBones()
        {
            Debug.Log("lazy " + this.name);
            var skinnedMeshRenderers = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            return skinnedMeshRenderers
                .First().bones
                .Where(x => !x.name.StartsWith("_"));
        }
    }

}

