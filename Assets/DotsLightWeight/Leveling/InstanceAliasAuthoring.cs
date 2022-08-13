using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;
    using DotsLite.Geometry.Palette;

    public class InstanceAliasAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        public ModelGroupAuthoring.ModelAuthoringBase ModelInstancePrefab;

        public ColorPaletteAsset Palette;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            if (!this.isActiveAndEnabled) return;
            if (this.ModelInstancePrefab == null) return;

            referencedPrefabs.Add(this.ModelInstancePrefab.gameObject);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }

            var gcs = conversionSystem;
            var em = gcs.DstEntityManager;
            em.AddComponentData(entity, new Spawn.EntryData
            {
                pos = this.transform.position,
                rot = this.transform.rotation,
                prefab = gcs.GetPrimaryEntity(this.ModelInstancePrefab),
                paletteIndex = this.Palette == null
                    ? -1
                    : gcs.GetColorPaletteBuilder().RegistAndGetId(this.Palette.Colors),
            });
        }
    }
}