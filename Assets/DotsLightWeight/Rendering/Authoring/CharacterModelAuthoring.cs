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

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw.Authoring;
    using DotsLite.Character;
    using DotsLite.Common.Extension;
    using DotsLite.CharacterMotion.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;

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
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            var top = this;
            var posture = this.GetComponentInChildren<PostureAuthoring>();
            var bones = this.QueryModel.First().Bones;

            this.QueryModel.CreateMeshAndModelEntitiesWithDictionary(conversionSystem);

            initBinderEntity_(conversionSystem, top, posture);

            conversionSystem.InitPostureEntity(posture);//, bones);
            conversionSystem.InitBoneEntities(posture, bones, root: posture.transform, this.BoneMode);

            conversionSystem.CreateDrawInstanceEntities(top, posture, bones, this.BoneMode);

            return;




            static void initBinderEntity_
                (GameObjectConversionSystem gcs_, CharacterModelAuthoring top_, PostureAuthoring posture)
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(posture);

                var types = new ComponentTypes
                (
                    typeof(BinderTrimBlankLinkedEntityGroupTag),
                    typeof(ObjectBinder.MainEntityLinkData)
                );
                em_.AddComponents(binderEntity, types);


                em_.SetComponentData(binderEntity,
                    new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


                em_.SetName_(binderEntity, $"{top_.name} binder");
            }

        }


    }

}

