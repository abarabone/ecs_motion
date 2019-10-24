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

            var qNameAndBone = motionClip.StreamPaths
                .Select( x => System.IO.Path.GetFileName( x ) )
                .Select( ( name, i ) => (name, i: motionClip.IndexMapFbxToMotion[ i ]) )
                .Where( x => x.i != -1 )
                .Select( x => (x.name, i: bonePrefabs[ x.i ]) );
                //.ToDictionary( x => x.name, x => x.i );

            var cs = this.GetComponentsInChildren<Collider>();
            var qColliders =
                from collider in cs
                let parent = collider.gameObject
                    .AncestorsAndSelf()
                    .Where( anc => anc.GetComponent<Rigidbody>() != null )
                    .First()
                select (collider, parent)
                ;

            var q =
                from c in qColliders
                join b in qNameAndBone
                on c.collider.name equals b.name
                select new

                

        }
    }
    // 剛体のないコライダは静的として変換する
    // モーションと同名のオブジェクトは、該当するボーンのエンティティにコンポーネントデータを付加する。
    // 剛体のついていないコライダは、一番近い先祖剛体に合成
}
