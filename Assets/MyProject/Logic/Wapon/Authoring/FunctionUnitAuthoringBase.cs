using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;

namespace Abarabone.Arms.Authoring
{
    public abstract class FunctionUnitAuthoringBase : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public abstract void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs);

        public abstract void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem);

    }
}
