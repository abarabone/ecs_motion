using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace DotsLite.EntityTrimmer.Authoring
{

    /// <summary>
    /// 空の子エンティティや存在しないエンティティへのリンクをトリムする。
    /// LinkedEntityGroup のトリムと、子エンティティの破棄を行う。
    /// prefab がついていても処理される。
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class TrimLinkedEntityConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
        //}
        //protected override void OnDestroy()
        //{
            var em = this.DstEntityManager;

            //var desc0 = new EntityQueryDesc
            //{
            //    All = new ComponentType[]
            //    {
            //        //typeof(BinderTrimBlankLinkedEntityGroupTag),
            //        typeof(LinkedEntityGroup),
            //        typeof(Prefab)
            //    }
            //};
            //var desc1 = new EntityQueryDesc
            //{
            //    All = new ComponentType[]
            //    {
            //        //typeof(BinderTrimBlankLinkedEntityGroupTag),
            //        typeof(LinkedEntityGroup)
            //    }
            //};
            var desc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(LinkedEntityGroup),
                },
                Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled,
            };

            using var q = em.CreateEntityQuery(desc);// desc0, desc1);
            using var ents = q.ToEntityArray(Allocator.TempJob);

            var needs = new NativeList<LinkedEntityGroup>(Allocator.Temp);
            foreach (var ent in ents)
            {
                var buf = em.GetBuffer<LinkedEntityGroup>(ent);

                //Debug.Log("enum in trim "+em.GetName_(ent));
                foreach (var link in buf)
                {
                    if (em.Exists(link.Value))
                    {
                        needs.Add(link);
                    }
                }

                if (needs.Length > 0)
                {
                    buf.Clear();
                    buf.AddRange(needs.AsArray());
                }

                needs.Clear();
            }
            

            needs.Dispose();

            //base.OnDestroy();
        }

        //protected override void OnUpdate()
        //{ }
    }

}