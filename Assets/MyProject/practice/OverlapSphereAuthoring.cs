using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace Abarabone.Physics.Authoring
{
    public class OverlapSphereAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public Vector3 Start;
        public Vector3 Direction;
        public float Length;
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

        }

    }
}
