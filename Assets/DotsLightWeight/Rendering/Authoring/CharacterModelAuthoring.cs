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

                this.QueryModel.Objs().PackTextureToDictionary(atlasDict);
                this.QueryModel.CreateModelToDictionary(meshDict, atlasDict, this.bones);
                this.QueryModel.CreateModelEntities(conversionSystem, meshDict, atlasDict, this.bones);


                //this.QueryOmmts.Objs().PackTextureToDictionary(atlasDict);

                //combineMeshToDictionary_();

                //createModelEntities_();

                return;


                //void combineMeshToDictionary_()
                //{
                //    using var meshAll = this.QueryOmmts.QueryMeshDataFromModel();

                //    var ofs = this.BuildMeshCombiners(meshAll.AsEnumerable, meshDict, atlasDict);
                //    var qMObj = ofs.Select(x => x.obj);
                //    var qMesh = ofs.Select(x => x.f.ToTask())
                //        .WhenAll().Result
                //        .Select(x => x.CreateMesh());
                //    //var qMesh = ofs.Select(x => x.f().CreateMesh());
                //    meshDict.AddRange(qMObj, qMesh);
                //}

                //void createModelEntities_()
                //{
                //    var objs = this.QueryOmmts.Objs();

                //    foreach (var obj in objs)
                //    {
                //        Debug.Log($"{obj.name} model ent");

                //        var mesh = meshDict[obj];
                //        var atlas = atlasDict.objectToAtlas[obj];
                //        createModelEntity_(obj, mesh, atlas);
                //    }

                //    return;


                //    void createModelEntity_(GameObject obj, Mesh mesh, Texture2D atlas)
                //    {
                //        var mat = new Material(shader);
                //        mat.enableInstancing = true;
                //        mat.mainTexture = atlas;

                //        const BoneType BoneType = BoneType.TR;

                //        gcs.CreateDrawModelEntityComponents(top, mesh, mat, BoneType, bones.Length);
                //    }
                //}

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






        //public override (GameObject obj, Func<IMeshElements> f)[] BuildMeshCombiners
        //    (
        //        IEnumerable<SrcMeshesModelCombinePack> meshpacks,
        //        Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
        //    )
        //{
        //    var qObjAndBuilder =
        //        from src in meshpacks
        //        where !meshDictionary.ContainsKey(src.obj)
        //        let atlas = atlasDictionary.objectToAtlas[src.obj].GetHashCode()
        //        let texdict = atlasDictionary.texHashToUvRect
        //        select (
        //            src.obj,
        //            src.BuildCombiner<UI32, PositionNormalUvBonedVertex>
        //                (part => texdict[atlas, part], this.bones)
        //        );
        //    return qObjAndBuilder.ToArray();
        //}




        //IEnumerable<ObjectAndMmts> _ommtss = null;

        //public override IEnumerable<ObjectAndMmts> QueryOmmts =>  this._ommtss ??=
        //    from obj in this.combineTargetObjects().ToArray()
        //    select new ObjectAndMmts
        //    {
        //        obj = obj,
        //        mmts = obj.QueryMeshMatsTransform_IfHaving().ToArray(),
        //    };

        //IEnumerable<GameObject> combineTargetObjects()
        //{
        //    return this.gameObject.WrapEnumerable();// 後でLODに対応させよう、とりあえずは単体で
        //}



        public override IEnumerable<IMeshModel> QueryModel => new[]
        {
            new CharacterModel<UI32, PositionNormalUvBonedVertex>
            {
                ObjectTop = this.gameObject,
                Shader = this.DrawShader,
            }
        };




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




    [Serializable]
    public class CharacterModel<TIdx, TVtx> : IMeshModel//IMeshModelLod
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {

        public GameObject ObjectTop;
        public Transform[] BoneRoot;

        public float LimitDistance;
        public float Margin;

        public Shader Shader;


        public GameObject obj => this.ObjectTop;
        public Transform[] bones => this.BoneRoot;
        public float limitDistance => this.limitDistance;
        public float margin => this.margin;



        public void CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.Shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType BoneType = BoneType.TR;
            var boneLength = bones.Length;

            gcs.CreateDrawModelEntityComponents(this.obj, mesh, mat, BoneType, boneLength);
        }

        public (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner
            (
                SrcMeshesModelCombinePack meshpack,
                Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary,
                Transform tfRoot = null
            )
        {
            //if (!meshDictionary.ContainsKey(this.ObjectTop)) return default;

            var atlas = atlasDictionary.objectToAtlas[this.ObjectTop].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            return (
                this.ObjectTop,
                meshpack.BuildCombiner<TIdx, TVtx>(tfRoot, part => texdict[atlas, part], this.bones)
            );
        }
    }


}

