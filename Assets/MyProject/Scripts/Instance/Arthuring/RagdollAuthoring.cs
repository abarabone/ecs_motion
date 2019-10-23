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
    public class RagdollAuthoring : MonoBehaviour
    {
        

        public void Convert
            ( EntityManager em, NativeArray<Entity> bonePrefabs )
        {

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//



        }
    }
    // ラグドールはボーンとは独立させて作成する。ブレンド対象
}