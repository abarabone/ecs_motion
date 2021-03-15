using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

namespace Abarabone.Model.Authoring
{

    /// <summary>
    /// TransformConversion をオフにする。
    /// 正式に紹介された方法ではないので、いつまで通用するかはわからない。
    /// ハイブリッドレンダラも使えなくなるし、WithOutTransformConversionAttribute みたいなのが待たれる…。
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateAfter(typeof(RemoveTransformComponentsConversion))]
    public class DisableTransformConversion : GameObjectConversionSystem
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            foreach( var x in this.World.Systems )
            {
                //Debug.Log($"{x}");

                if ( x.ToString() == "TransformConversion")
                {
                    x.Enabled = false;

                    //this.World.GetExistingSystem<RemoveTransformComponentsConversion>().Enabled = false;

                    Debug.Log($"{x} disabled");
                    break;
                }

            }
        }

        protected override void OnUpdate()
        { }
    }

}