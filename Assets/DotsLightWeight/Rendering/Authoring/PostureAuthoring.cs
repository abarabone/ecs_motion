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

namespace Abarabone.Model.Authoring
{
    using Abarabone.Draw.Authoring;
    using Abarabone.Character;
    using Abarabone.Common.Extension;
    using Abarabone.CharacterMotion.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Utilities;


    public class PostureAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public EnBoneType BoneMode;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {



        }
    }
}