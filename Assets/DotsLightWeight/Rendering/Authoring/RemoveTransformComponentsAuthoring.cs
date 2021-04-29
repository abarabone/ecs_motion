using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

namespace DotsLite.Model.Authoring
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoveTransformComponentsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public void Convert
            (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

        }
    }
}
