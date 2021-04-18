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


        IEnumerable<IMeshModel> _models = null;

        public override IEnumerable<IMeshModel> QueryModel => _models ??= new CharacterModel<UI32, PositionNormalUvBonedVertex>
        (
            this.gameObject,
            this.DrawShader,
            this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().bones.First()
        )
        .WrapEnumerable();




        /// <summary>
        /// 描画関係はバインダーに、ボーン関係はメインに関連付ける
        /// ステートは Mesh に関連付ける（苦し紛れ）
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject;
            var main = top.Children().First();
            var bones = this.QueryModel.First().Bones;

            this.QueryModel.CreateMeshAndModelEntitiesWithDictionary(conversionSystem);

            initBinderEntity_(conversionSystem, top, main);
            initPostureEntity_(conversionSystem, top, main);

            conversionSystem.InitPostureEntity(main);//, bones);
            conversionSystem.InitBoneEntities(main, bones, root: main.transform, this.BoneMode);

            conversionSystem.CreateDrawInstanceEntities(top, main, bones, this.BoneMode);

            return;




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

            static void initPostureEntity_(GameObjectConversionSystem gcs, GameObject top, GameObject main)
            {
                var em = gcs.DstEntityManager;

                var binderEntity = gcs.GetPrimaryEntity(top);
                var mainEntity = gcs.GetPrimaryEntity(main);

                var mainAddtypes = new ComponentTypes
                (

                );
                em.AddComponents(mainEntity, mainAddtypes);


                em.SetName_(mainEntity, $"{top.name} main");
            }

            static void initStateEntity_(GameObjectConversionSystem gcs, GameObject top, GameObject state)
            {
                var em = gcs.DstEntityManager;

                var binderEntity = gcs.GetPrimaryEntity(top);
                var stateEntity = gcs.GetPrimaryEntity(state);

                var mainAddtypes = new ComponentTypes
                (
                    //typeof(ObjectMain.ObjectMainTag),
                    typeof(ObjectMain.BinderLinkData)
                );
                em.AddComponents(stateEntity, mainAddtypes);


                em.SetComponentData(stateEntity,
                    new ObjectMain.BinderLinkData
                    {
                        BinderEntity = binderEntity,
                    }
                );
                //em_.SetComponentData(mainEntity,
                //    new ObjectMainCharacterLinkData
                //    {
                //        PostureEntity = mainEntity,//
                //    }
                //);


                em.SetName_(stateEntity, $"{top.name} state");
            }

        }


    }

}

