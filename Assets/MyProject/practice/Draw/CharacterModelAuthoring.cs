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

            var modelEntity = createModelEntity_( dstManager, this.MaterialToDraw, qMesh, bones );

            conversionSystem.CreateBoneEntities( dstManager, this.gameObject, bones );

            return;


            Entity createModelEntity_( EntityManager em, Material srcMaterial, IEnumerable<Mesh> srcMeshes, Transform[] bones_ )
            {
                var mat = new Material( srcMaterial );
                var mesh = DrawModelEntityConvertUtility.CombineAndConvertMesh( srcMeshes, bones_ );

                const Draw.BoneType boneType = Draw.BoneType.TR;

                var modelEntity_ = em.CreateDrawModelEntityComponents( mesh, mat, boneType, bones_.Length );
                dstManager.SetName( modelEntity_, $"{this.name} model" );

                return modelEntity_;
            }

        }

    }



}

