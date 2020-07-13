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
    /// 標準のコンバートだと、単一のエンティティでも LinkedEntityGroup を持ってしまう。
    /// いらない場合は、あとから消すしかなさそう。なので、そのタグ。
    /// </summary>
    public struct ModelPrefabNoNeedLinkedEntityGroupTag : IComponentData
    { }


    /// <summary>
    /// Prefab しか持っていない子エンティティが不要なら、トリムする。
    /// </summary>
    public struct BinderTrimBlankLinkedEntityGroupTag : IComponentData
    { }

}
