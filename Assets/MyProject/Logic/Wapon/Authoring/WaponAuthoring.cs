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

    public partial class WaponAuthoring : MonoBehaviour, IWaponAuthoring, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public IFunctionUnitAuthoring MainUnit;
        public IFunctionUnitAuthoring SubUnit;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            (this.MainUnit as IDeclareReferencedPrefabs)?.DeclareReferencedPrefabs(referencedPrefabs);
            (this.SubUnit as IDeclareReferencedPrefabs)?.DeclareReferencedPrefabs(referencedPrefabs);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var top = this.gameObject
                .Ancestors()
                .First(go => go.GetComponent<CharacterModelAuthoring>());

            var mainEntity = conversionSystem.CreateAdditionalEntity(top);
            (this.MainUnit as IConvertGameObjectToEntity)?.Convert(mainEntity, dstManager, conversionSystem);

            var subEntity = conversionSystem.CreateAdditionalEntity(top);
            (this.SubUnit as IConvertGameObjectToEntity)?.Convert(subEntity, dstManager, conversionSystem);

            dstManager.DestroyEntity(entity);
        }
    }

}
