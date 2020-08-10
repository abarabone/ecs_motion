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

    public class WaponAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public class EmitUnitAuthoring : MonoBehaviour
        { }


        public EmitUnitAuthoring[] EmitUnits;




        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var units = this.EmitUnits
                .Where(x => x != null)
                .Select(x => x.gameObject);

            referencedPrefabs.AddRange(units);
        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            initWapon_(conversionSystem, entity);

            initEmitUnit_(conversionSystem, entity);

            return;


            void initWapon_(GameObjectConversionSystem gcs_, Entity wapon_)
            {
                var em = gcs_.DstEntityManager;

                var addtypes = new ComponentTypes
                (
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(Rotation),
                    typeof(Translation)
                );
                em.AddComponents(wapon_, addtypes);
            }

            void initEmitUnit_(GameObjectConversionSystem gcs_, Entity wapon_)
            {
                var units = this.EmitUnits
                    .Where(x => x != null)
                    .Select(x => conversionSystem.GetPrimaryEntity(x))
                    .ToArray();

                switch (units.Length)
                {
                    case 1:

                        dstManager.AddComponentData(entity,
                            new Wapon.Unit1HolderData
                            {
                                UnitEntity0 = units[0],
                            }
                        );

                        break;
                    case 2:

                        dstManager.AddComponentData(entity,
                            new Wapon.Unit2HolderData
                            {
                                UnitEntity0 = units[0],
                                UnitEntity1 = units[1],
                            }
                        );

                        break;
                }
            }
            
        }
    }

}
