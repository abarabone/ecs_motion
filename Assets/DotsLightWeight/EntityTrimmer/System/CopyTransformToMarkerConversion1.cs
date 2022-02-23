using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics.Authoring;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;
using System;

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
    [UpdateBefore(typeof(RemoveTransformAllConversion))]
    public class CopyTransformToMarkerConversion1 : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .ForEach((Entity e, Transform tf) =>
                {
                    if (tf.)

                    Debug.Log(tf.name);

                });
        }
    }

}
