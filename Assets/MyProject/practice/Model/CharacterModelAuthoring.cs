using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Model.Authoring
{
    using Draw.Authoring;
    using Abarabone.Common.Extension;

    /// <summary>
    /// 
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




        /// <summary>
        /// プライマリエンティティは LinkedEntityGroup のみとする。
        /// メインエンティティは新しく作る。ただし physics 関連のコンポーネントがプライマリについてしまうので、あとでつけかえる…（嫌だなぁ…）
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var skinnedMeshRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>();
            var qMesh = skinnedMeshRenderers.Select( x => x.sharedMesh );
            var bones = skinnedMeshRenderers.First().bones.Where( x => !x.name.StartsWith( "_" ) ).ToArray();

            createModelEntity_( conversionSystem, this.gameObject, this.MaterialToDraw, qMesh, bones );

            initObjectEntity_( conversionSystem, this.gameObject );

            conversionSystem.CreateBoneEntities( this.gameObject, bones );

            conversionSystem.CreateDrawInstanceEntities( this.gameObject, bones );

            return;


            void createModelEntity_
                (
                    GameObjectConversionSystem gcs_, GameObject main_,
                    Material srcMaterial, IEnumerable<Mesh> srcMeshes, Transform[] bones_
                )
            {
                var mat = new Material( srcMaterial );
                var mesh = DrawModelEntityConvertUtility.CombineAndConvertMesh( srcMeshes, bones_ );

                const Draw.BoneType boneType = Draw.BoneType.TR;

                gcs_.CreateDrawModelEntityComponents( main_, mesh, mat, boneType, bones_.Length );
            }


            void initObjectEntity_( GameObjectConversionSystem gcs_, GameObject main_ )
            {
                var em_ = gcs_.DstEntityManager;

                var mainEntity = CharacterModelAuthoring.GetOrCreateMainEntity(gcs_, main_);
                var binderEntity = gcs_.GetPrimaryEntity(main_);

                em_.AddComponentData( binderEntity,
                    new BinderObjectMainEntityLinkData { MainEntity = mainEntity } );

                em_.AddComponentData( mainEntity, new ObjectMainEntityTag { } );
                em_.SetComponentData( mainEntity,
                    new ObjectBinderLinkData { BinderEntity = binderEntity} );

                em_.SetName( mainEntity, $"{main_.name} main" );
            }

        }



        static public Entity GetOrCreateMainEntity( GameObjectConversionSystem gcs, GameObject main )
        {
            var em = gcs.DstEntityManager;

            var mainEntity = gcs.GetEntities( main )
                .Where( ent_ => em.HasComponent<ObjectBinderLinkData>( ent_ ) )
                .FirstOrDefault();

            if( mainEntity == Entity.Null )
                return gcs.CreateAdditionalEntity<ObjectBinderLinkData>( main );

            return mainEntity;
        }

    }



}

