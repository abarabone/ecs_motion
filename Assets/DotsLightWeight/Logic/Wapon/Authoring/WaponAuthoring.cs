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
    using Abarabone.Model.Authoring;
    using Abarabone.Character;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Draw;
    using Abarabone.CharacterMotion;
    using Abarabone.Arms;
    using Unity.Physics.Authoring;
    using Abarabone.Model;

    public partial class WaponAuthoring : MonoBehaviour, IWaponAuthoring//, IDeclareReferencedPrefabs//, IConvertGameObjectToEntity//
    {

        //public IFunctionUnitAuthoring MainUnit;
        //public IFunctionUnitAuthoring SubUnit;

        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    var units = this.GetComponentsInChildren<IFunctionUnitAuthoring>()
        //        .Cast<MonoBehaviour>()
        //        .Select(x => x.gameObject);

        //    foreach (var unit in units)
        //    {
        //        referencedPrefabs.Add(unit);
        //    }
        //}

        //public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        //{

        //    dstManager.DestroyEntity(entity);
        //}
    }

}
