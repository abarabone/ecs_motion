using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace DotsLite.Model.Authoring
{

    /// <summary>
    /// 空の子エンティティや存在しないエンティティへのリンクをトリムする。
    /// LinkedEntityGroup のトリムと、子エンティティの破棄を行う。
    /// prefab がついていても処理される。
    /// </summary>
    //[DisableAutoCreation]
    //[UpdateBefore(typeof(NoNeedLinkedEntityGroupCleanUpConversion))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class TrimBlankEntityFromLinkedEntityGroupConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        { }
        protected override void OnDestroy()
        {
            var em = this.DstEntityManager;
            var needs = new NativeList<LinkedEntityGroup>(Allocator.Temp);
            var noneeds = new NativeList<LinkedEntityGroup>(Allocator.Temp);

            var desc0 = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    //typeof(BinderTrimBlankLinkedEntityGroupTag),
                    typeof(LinkedEntityGroup),
                    typeof(Prefab)
                }
            };
            var desc1 = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    //typeof(BinderTrimBlankLinkedEntityGroupTag),
                    typeof(LinkedEntityGroup)
                }
            };

            using (var q = em.CreateEntityQuery(desc0, desc1))
            using (var ents = q.ToEntityArray(Allocator.TempJob))
            {
                foreach (var ent in ents)
                {
                    var buf = em.GetBuffer<LinkedEntityGroup>(ent);

                    //Debug.Log("enum in trim "+em.GetName_(ent));
                    foreach (var link in buf)
                    {
                        if (em.GetComponentCount(link.Value) == 1 && em.HasComponent<Prefab>(link.Value))
                        {
                            noneeds.Add(link);
                        }
                        else if (em.GetComponentCount(link.Value) == 0)
                        {
                            noneeds.Add(link);
                        }
                        else if (em.Exists(link.Value))
                        {
                            needs.Add(link);
                        }
                        //else if (!em.Exists(link.Value))
                        //{
                        //    noneeds.Add(link);
                        //}
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

            //base.OnDestroy();
        }

        //protected override void OnUpdate()
        //{ }
    }

}