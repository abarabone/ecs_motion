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
    using Abarabone.Draw;

    public class CharacterMotionAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        /// <summary>
        /// 
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {



        }
    }
}
