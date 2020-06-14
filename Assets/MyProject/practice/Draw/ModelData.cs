using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model
{

    /// <summary>
    /// プレハブとして Instantiate() すべき Entity を格納する
    /// GameObjectConversion で、Entity １つの時でもいらん LinkedEntityGroup がついてしまうため
    /// コンバート時プレハブと実使用プレハブは別にする（2つ以上の場合はリンク必要なので別にしない）
    /// トランスフォームとかいらんもん強制しすぎ
    /// </summary>
    public struct ModelPrefabHeadData : IComponentData
    {
        public Entity PrefabHeadEntity;
    }

}
