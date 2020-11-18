using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Arms.Authoring
{
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;

    public class FunctionHolderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var armsFunctions = this.GetComponentsInChildren<IFunctionUnitAuthoring>();



        }
    }
}