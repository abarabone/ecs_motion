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
    using Draw.Authoring;
    using Abarabone.Common.Extension;

    /// <summary>
    /// プライマリエンティティは LinkedEntityGroup のみとする。
    /// その１つ下に、ＦＢＸのキャラクターを置く。それがメインエンティティとなる。
    /// </summary>
    public class CharacterModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public Material MaterialToDraw;

        //public bool CreateAtlusTexture;

        
        public EnBoneType Mode;
        public enum EnBoneType
        {
            reelup_chain,
            jobs_per_depth,
        }




        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var skinnedMeshRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>();
            var qMesh = skinnedMeshRenderers.Select( x => x.sharedMesh );
            var bones = skinnedMeshRenderers.First().bones.Where( x => !x.name.StartsWith( "_" ) ).ToArray();

            var top = this.gameObject;
            var main = transform.GetChild(0).gameObject;

            createModelEntity_( conversionSystem, top, this.MaterialToDraw, qMesh, bones );

            initObjectEntity_( conversionSystem, top, main );

            conversionSystem.CreateBoneEntities( main, bones );

            conversionSystem.CreateDrawInstanceEntities( top, bones );

            return;


            void createModelEntity_
                (
                    GameObjectConversionSystem gcs_, GameObject top_,
                    Material srcMaterial, IEnumerable<Mesh> srcMeshes, Transform[] bones_
                )
            {
                var mat = new Material( srcMaterial );
                var mesh = DrawModelEntityConvertUtility.CombineAndConvertMesh( srcMeshes, bones_ );

                const Draw.BoneType boneType = Draw.BoneType.TR;

                gcs_.CreateDrawModelEntityComponents( top_, mesh, mat, boneType, bones_.Length );
            }

            void initObjectEntity_( GameObjectConversionSystem gcs_, GameObject top_, GameObject main_ )
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(main_);


                em_.AddComponentData( binderEntity,
                    new BinderObjectMainEntityLinkData { MainEntity = mainEntity } );


                var addtypes = new ComponentTypes
                (
                    typeof(ObjectMainEntityTag),
                    typeof(ObjectBinderLinkData)
                );
                em_.AddComponents(mainEntity, addtypes);

                em_.SetComponentData( mainEntity,
                    new ObjectBinderLinkData { BinderEntity = binderEntity} );


                em_.SetName( binderEntity, $"{top_.name} binder" );
                em_.SetName( mainEntity, $"{top_.name} main" );
            }

        }


    }

}

