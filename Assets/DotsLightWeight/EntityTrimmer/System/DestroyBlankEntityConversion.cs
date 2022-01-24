using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics.Authoring;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;
using Unity.Collections;

namespace DotsLite.EntityTrimmer.Authoring
{
    using Utilities;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateBefore(typeof(TrimLinkedEntityConversion))]
    public class DestroyBlankEntityConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            // 空のエンティティだけ取得できるクエリかなんかないんかな
            var desc = new EntityQueryDesc
            {
                Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
            };
            using var q = em.CreateEntityQuery(desc);

            using var ents = q.ToEntityArray(Allocator.Temp);
            foreach (var ent in ents)
            {
                switch(em.GetComponentCount(ent))
                {
                    case 2:
                        if (em.HasComponent<Prefab>(ent) && em.HasComponent<Disabled>(ent)) break;
                        continue;
                    case 1:
                        if (em.HasComponent<Prefab>(ent)) break;
                        if (em.HasComponent<Disabled>(ent)) break;
                        continue;
                    case 0:
                        break;
                    default:
                        continue;
                }

                em.DestroyEntity(ent);
            }
        }
    }

}
