using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Arthuring
{
    public class CharacterModelAuthoring : MonoBehaviour, IModelAuthoring, IConvertGameObjectToEntity
    {


        public Shader Shader;



        GameObject IModelAuthoring.GameObject => this.gameObject;


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            

        }

    }
}

