using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Model.Authoring
{

    /// <summary>
    /// 空の子エンティティをトリムする。
    /// LinkedEntityGroup のトリムと、子エンティティの破棄を行う。
    /// </summary>
    //[DisableAutoCreation]
    public class TrimBlankEntityFromLinkedEntityGroupSystem : GameObjectConversionSystem
    {
        protected override void OnDestroy()
        {
            var em = this.DstEntityManager;
            var needs = new NativeList<LinkedEntityGroup>( Allocator.Temp );
            var noneeds = new NativeList<LinkedEntityGroup>( Allocator.Temp );

            using (var q = em.CreateEntityQuery(
                    typeof(BinderTrimBlankLinkedEntityGroupTag),
                    typeof(LinkedEntityGroup),
                    typeof(Prefab)// ありでもなしでもかかるようにしたいが、方法がわからん
            ))
            using (var ents = q.ToEntityArray(Allocator.TempJob))
            {
                foreach (var ent in ents)
                {
                    var buf = em.GetBuffer<LinkedEntityGroup>(ent);

                    Debug.Log("enum in trim "+em.GetName_(ent));
                    foreach (var link in buf)
                    {
                        if (em.GetComponentCount(link.Value) == 1 && em.HasComponent<Prefab>(link.Value))
                        {
                            noneeds.Add(link);
                        }
                        else if( em.GetComponentCount(link.Value) == 0 )
                        {
                            noneeds.Add(link);
                        }
                        else
                        {
                            needs.Add(link);
                        }
                    }

                    if (needs.Length > 0)
                    {
                        buf.Clear();
                        buf.AddRange(needs.AsArray());
                    }
                    if (noneeds.Length > 0)
                    {
                        em.DestroyEntity(noneeds.AsArray().Reinterpret<Entity>());
                    }

                    needs.Clear();
                    noneeds.Clear();
                }
            }

            needs.Dispose();
            noneeds.Dispose();

            base.OnDestroy();
        }

        protected override void OnUpdate()
        { }
    }

}