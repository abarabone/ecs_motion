using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Authoring
{
    public class CharacterModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public Material Material;


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            

        }

    }
}

