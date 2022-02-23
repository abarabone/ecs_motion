using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace DotsLite.EntityTrimmer.Authoring
{
    using Utilities;

    /// <summary>
    /// TransformConversion によって付与される、トランスフォーム系のコンポーネントデータを削除する。
    /// ExcludeTransformConversion とか はよ
    /// </summary>
    /// transform conversion は [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    //[UpdateBefore(typeof(AddTransformConversion))]
    public class RemoveTransformAllConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            //this.Entities
            //    .WithIncludeAll()
            //    .WithNone<ExcludeRemoveTransform>()
            //    .ForEach
            //(
            //    (Transform tf) =>
            //    {
            //        //Debug.Log($"remove all {tf.name}");
            //        var ent = this.GetPrimaryEntity(tf);
            //        em.RemoveComponent<LocalToParent>(ent);
            //        em.RemoveComponent<LocalToWorld>(ent);
            //        em.RemoveComponent<PreviousParent>(ent);
            //        em.RemoveComponent<Parent>(ent);
            //        em.RemoveComponent<Child>(ent);
            //        em.RemoveComponent<Translation>(ent);
            //        em.RemoveComponent<Rotation>(ent);
            //        em.RemoveComponent<NonUniformScale>(ent);
            //        em.RemoveComponent<Scale>(ent);
            //        em.RemoveComponent<CompositeScale>(ent);
            //    }
            //);

            {
                var desc = new EntityQueryDesc
                {
                    None = new ComponentType[]
                    {
                        typeof(ExcludeRemoveTransform),
                    },
                    Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled,
                };
                //var desc1 = new EntityQueryDesc
                //{
                //    None = new ComponentType[]
                //    {
                //        typeof(PhysicsJoint),
                //    },
                //    Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled,
                //};

                using var q = em.CreateEntityQuery(desc);

                em.RemoveComponent<LocalToParent>(q);
                em.RemoveComponent<LocalToWorld>(q);
                em.RemoveComponent<PreviousParent>(q);
                em.RemoveComponent<Parent>(q);
                em.RemoveComponent<Child>(q);
                em.RemoveComponent<Translation>(q);
                em.RemoveComponent<Rotation>(q);
                em.RemoveComponent<NonUniformScale>(q);
                em.RemoveComponent<Scale>(q);
            }
            {
                var desc = new EntityQueryDesc
                {
                    All = new ComponentType[]
                    {
                        typeof(ExcludeRemoveTransform),
                    },
                    Options = EntityQueryOptions.IncludePrefab | EntityQueryOptions.IncludeDisabled,
                };

                using var q = em.CreateEntityQuery(desc);

                em.RemoveComponent<ExcludeRemoveTransform>(q);
            }
        }
    }

}
