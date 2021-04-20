using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Unity.Physics;
using System;
using System.Runtime.InteropServices;

namespace Abarabone.Arms
{

    static public partial class FunctionHolder
    {

        [InternalBufferCapacity(4)]
        public struct LinkData : IBufferElementData
        {
            public Entity FunctionEntity;
        }

    }

    static public partial class WaponHolder
    {

        public struct SelectorData : IComponentData
        {
            public int Length;
            public int CurrentWaponIndex;
        }

        public struct OwnerLinkData : IComponentData
        {
            //public Entity OwnerEntity;
            public Entity MuzzleEntity;
        }
        public struct StateLinkData : IComponentData
        {
            public Entity StateEntity;
        }


        // 持っておく必要ないかも
        [InternalBufferCapacity(4)]
        public struct UnitLinkData : IBufferElementData
        {
            public Entity FunctionEntity0;
            public Entity FunctionEntity1;
        }

    }

    static public partial class WaponTemplate
    {


        // 武器のテンプレート
        public struct UnitsData : IComponentData
        {
            public Entity FunctionEntity0;
            public Entity FunctionEntity1;
        }
        // ↓いずれ必要になれば作ろう
        //public struct ModelLinkData : IComponentData
        //{
        //    public Entity DrawModelEntity;
        //}


        //// WaponHolder.LinkData に追加する武器のひな型エンティティを指定する。
        //// 既存のエンティティに存在していてもよいし、
        //// エンティティを新設してくっつけてもよい（その場合は処理後にエンティティが破棄される）
        //public struct AddWaponData : IComponentData
        //{
        //    public Entity HolderEntity;
        //    public Entity TemplateWaponEntity0;
        //    public Entity TemplateWaponEntity1;
        //    public Entity TemplateWaponEntity2;
        //    public Entity TemplateWaponEntity3;
        //}
    }


}
