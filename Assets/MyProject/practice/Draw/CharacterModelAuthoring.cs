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


        //public AvatarMask BoneMask;
        //public Transform[] BoneRoots;
        
        public EnBoneType Mode;
        public enum EnBoneType
        {
            reel_a_chain,
            in_deep_order,// jobs_per_depth,
        }




        /// <summary>
        /// 
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var skinnedMeshRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>();
            var qMesh = skinnedMeshRenderers.Select( x => x.sharedMesh );
            var bones = skinnedMeshRenderers.First().bones.Where( x => !x.name.StartsWith( "_" ) ).ToArray();

            createModelEntity_( conversionSystem, dstManager, this.gameObject, this.MaterialToDraw, qMesh, bones );

            createMainEntity_( conversionSystem, dstManager, this.gameObject );

            conversionSystem.CreateBoneEntities( dstManager, this.gameObject, bones );

            conversionSystem.CreateDrawInstanceEntities( dstManager, this.gameObject, bones );

            return;


            void createModelEntity_
                (
                    GameObjectConversionSystem gcs_, EntityManager em_, GameObject main_,
                    Material srcMaterial, IEnumerable<Mesh> srcMeshes, Transform[] bones_
                )
            {
                var mat = new Material( srcMaterial );
                var mesh = DrawModelEntityConvertUtility.CombineAndConvertMesh( srcMeshes, bones_ );

                const Draw.BoneType boneType = Draw.BoneType.TR;

                gcs_.CreateDrawModelEntityComponents( em_,  main_, mesh, mat, boneType, bones_.Length );
            }

            void createMainEntity_
                ( GameObjectConversionSystem gcs_, EntityManager em_, GameObject main_ )
            {
                var mainEntity = CharacterModelAuthoring.GetOrCreateMainEntity( gcs_, em_, main_ );
                var binderEntity = gcs_.GetPrimaryEntity( main_ );

                em_.AddComponentData( binderEntity,
                    new ModelBinderLinkData { MainEntity = mainEntity } );

                em_.SetComponentData( mainEntity,
                    new ModelMainEntityLinkData { BinderEntity = binderEntity} );

                em_.SetName( mainEntity, $"{main_.name} main" );
            }
        }


        static public Entity GetOrCreateMainEntity
            ( GameObjectConversionSystem gcs, EntityManager em, GameObject main )
        {
            var mainEntity = gcs.GetEntities( main )
                .Where( ent_ => em.HasComponent<ModelMainEntityLinkData>( ent_ ) )
                .FirstOrDefault();

            if( mainEntity == Entity.Null )
                return gcs.CreateAdditionalEntity<ModelMainEntityLinkData>( em, main );

            return mainEntity;
        }

    }



}

