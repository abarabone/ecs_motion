using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Motion.Authoring
{
    public class MotionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public MotionClip MotionClip;

        public AvatarMask StreamMask;


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var bones = this.GetComponentInChildren<SkinnedMeshRenderer>().bones
                .Where( bone => !bone.name.StartsWith( "_" ) )
                .ToArray();

            var motionTypes = ArchetypeB.Motion;
            var streamTypes = ArchetypeB.Stream;

            conversionSystem.ConvertMotionEntities
                ( this.gameObject, bones, motionTypes, streamTypes, this.MotionClip, this.StreamMask );

        }

    }
}
