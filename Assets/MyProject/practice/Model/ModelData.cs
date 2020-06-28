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

    public struct BinderObjectMainEntityLinkData : IComponentData
    {
        public Entity MainEntity;
    }

    public struct ObjectBinderLinkData : IComponentData
    {
        public Entity BinderEntity;
    }

    public struct ObjectMainEntityTag : IComponentData
    { }

}
