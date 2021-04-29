using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace DotsLite.Model.Authoring
{

    /// <summary>
    /// TransformConversion によって付与される、トランスフォーム系のコンポーネントデータを削除する。
    /// ExcludeTransformConversion とか はよ
    /// </summary>
    /// transform conversion は [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class RemoveTransformComponentsConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities.ForEach
            (
                (Entity e, RemoveTransformComponentsAuthoring c) =>
                {
                    foreach (var tf in c.GetComponentsInChildren<Transform>())
                    {
                        //Debug.Log(tf.name);
                        var ent = this.GetPrimaryEntity(tf);

                        em.RemoveComponent<LocalToParent>(ent);
                        em.RemoveComponent<LocalToWorld>(ent);
                        em.RemoveComponent<PreviousParent>(ent);
                        em.RemoveComponent<Parent>(ent);
                        em.RemoveComponent<Child>(ent);
                        //em.RemoveComponent<Translation>(ent);
                        //em.RemoveComponent<Rotation>(ent);
                        //em.RemoveComponent<NonUniformScale>(ent);
                        //em.RemoveComponent<Scale>(ent);

                    }
                }
            );

        }
    }

}
