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
using Unity.Physics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Instance;
using Abss.Common.Extension;
using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using MeshCollider = Unity.Physics.MeshCollider;

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
                
            var qSrcs =
                from collider in this.GetComponentsInChildren<UnityEngine.Collider>()
                let tfParent = collider.gameObject
                    .AncestorsAndSelf()
                    .Where( anc => anc.GetComponent<Rigidbody>() != null )
                    .First()
                select (collider, tfParent)
                ;
            var srcs = qSrcs.ToArray();

            var pcs =
                from x in srcs
                select new PhysicsCollider
                {
                    Value = 
                };
            var pvs =
                from x in srcs
                select new PhysicsVelocity
                {

                };
            var pms =
                from x in srcs
                select new PhysicsMass
                {

                };
            var pds =
                from x in srcs
                select new PhysicsGravityFactor
                {

                };

            var q =
                from c in srcs
                join b in qNameAndBone
                on c.collider.name equals b.name
                select 1;

            BlobAssetReference<Unity.Physics.Collider> createCollider( UnityEngine.Collider srcCollider )
            {
                switch( srcCollider )
                {
                    case UnityEngine.SphereCollider srcSphere:

                        break;
                }
            }
        }
    }
    // 剛体のないコライダは静的として変換する
    // モーションと同名のオブジェクトは、該当するボーンのエンティティにコンポーネントデータを付加する。
    // 剛体のついていないコライダは、一番近い先祖剛体に合成
}
