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
    using Unity.Physics.Authoring;

namespace Abarabone.Arms.Authoring
{
    using Abarabone.Model.Authoring;
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;

    public class WaponGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {


        public WaponAuthoring[] Wapons;




        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {

            var wapons = this.Wapons
                .Where(x => x != null)
                .Select(x => x.gameObject);

            referencedPrefabs.AddRange(wapons);

        }


        public new void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.DestroyEntity(entity);

        }
    }
}

