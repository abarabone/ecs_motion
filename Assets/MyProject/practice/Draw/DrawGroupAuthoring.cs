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
    public interface IModelAuthoring
    {
        GameObject GameObject { get; }
    }
}

namespace Abarabone.Draw.Arthuring
{
    public class DrawGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {


        public Model.Arthuring.IModelAuthoring[] ModelPrefabs;



        void IDeclareReferencedPrefabs.DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            this.ModelPrefabs.ForEach( x => referencedPrefabs.Add(x.GameObject) );
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {



        }

    }
}
