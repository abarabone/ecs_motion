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

    /// <summary>
    /// WaponEntity はインスタンス化しない。
    /// FunctionUnit をインスタンス化するためのリファレンスでしかない。
    /// </summary>
    public class WaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public class FunctionUnitAuthoring : MonoBehaviour
        { }


        public FunctionUnitAuthoring[] EmitUnits;




        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var units = this.EmitUnits
                .Where(x => x != null)
                .Select(x => x.gameObject);

            referencedPrefabs.AddRange(units);
        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            addWaponComponents_(conversionSystem, entity);

            initFunctionUnit_(conversionSystem, entity);

            return;


            void addWaponComponents_(GameObjectConversionSystem gcs_, Entity wapon_)
            {
                var em = gcs_.DstEntityManager;

                var addtypes = new ComponentTypes
                (
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(Wapon.FunctionUnitPrefabsData),
                    typeof(Rotation),
                    typeof(Translation)
                );
                em.AddComponents(wapon_, addtypes);
            }

            void initFunctionUnit_(GameObjectConversionSystem gcs_, Entity wapon_)
            {
                var units = this.EmitUnits
                    .Where(x => x != null)
                    .Select(x => conversionSystem.GetPrimaryEntity(x))
                    .ToArray();

                var prefabs = new Wapon.FunctionUnitPrefabsData { };

                if (units.Length >= 1) prefabs.FunctionUnitPrefab0 = units[0];
                if (units.Length >= 2) prefabs.FunctionUnitPrefab0 = units[1];
                if (units.Length >= 3) prefabs.FunctionUnitPrefab0 = units[2];
                if (units.Length >= 4) prefabs.FunctionUnitPrefab0 = units[3];

                dstManager.SetComponentData(wapon_, prefabs);
            }

        }
    }

}
