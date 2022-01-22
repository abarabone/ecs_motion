using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
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
    //[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateAfter(typeof(RemoveTransformAllSystem))]
    public class AddTransformAllSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .ForEach
            (
                (Entity ent, TransformAuthoring authoring) =>
                {
                    var tf = authoring.transform;

                    if (authoring.Translation) em.AddComponentData(ent, new Translation
                    {
                        Value = tf.position,
                    });

                    if (authoring.Rotation) em.AddComponentData(ent, new Rotation
                    {
                        Value = tf.rotation,
                    });

                    if (authoring.Scale) em.AddComponentData(ent, new Scale
                    {
                        Value = tf.lossyScale.magnitude,
                    });

                    if (authoring.NonUniformScale) em.AddComponentData(ent, new NonUniformScale
                    {
                        Value = tf.lossyScale,
                    });

                    if (authoring.LocalToWorldMatrix) em.AddComponentData(ent, new LocalToWorld
                    {
                        Value = tf.localToWorldMatrix,
                    });
                }
            );

        }
    }

}
