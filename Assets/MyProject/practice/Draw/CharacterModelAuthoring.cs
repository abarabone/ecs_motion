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

        public Material Material;

        //public AvatarMask BoneMask;

        //public Transform[] BoneRoots;


        public EnBoneType Mode;
        public enum EnBoneType
        {
            reel_a_chain,
            in_deep_order,
        }


        /// <summary>
        /// 
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            var skinnedMeshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();

            var bones = skinnedMeshRenderer.bones.Where( x => !x.name.StartsWith( "_" ) ).ToArray();

            conversionSystem.CreateEntities( dstManager, this.gameObject, bones );
            
        }

    }



}

