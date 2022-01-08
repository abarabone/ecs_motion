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

namespace DotsLite.Arms.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Character;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.CharacterMotion;
    using DotsLite.Arms;

    public class WaponGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {


        public WaponAuthoring[] Wapons;




        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            //var functions = this.Wapons
            //    .Where(x => x != null)
            //    .Select(x => new[] { x.MainUnit.gameObject, x.SubUnit.gameObject })
            //    .SelectMany();

            //referencedPrefabs.AddRange(functions);

        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.DestroyEntity(entity);

        }
    }
}

