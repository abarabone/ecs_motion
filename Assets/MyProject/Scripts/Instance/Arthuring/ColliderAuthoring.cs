using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;
using Unity.Mathematics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Instance;
using Abss.Common.Extension;

namespace Abss.Arthuring
{
    public class ColliderAuthoring : MonoBehaviour
    {
        
        public void Convert
            ( EntityManager em, NativeArray<Entity> bonePrefabs )
        {

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//



        }
    }
    // 剛体のないコライダは静的として変換する
    // モーションと同名のオブジェクトは、該当するボーンのエンティティにコンポーネントデータを付加する。
    // 合成コライダはなし
}
