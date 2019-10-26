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
using Unity.Physics.Authoring;

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
                
            var qColliderwithParent =
                from collider in this.GetComponentsInChildren<UnityEngine.Collider>()
                let tfParent = collider.gameObject
                    .AncestorsAndSelf()
                    .Where( anc => anc.GetComponent<Rigidbody>() != null )
                    .First().transform
                select (collider, tfParent)
                ;
            var collidersWithParent = qColliderwithParent.ToArray();

            var blobRefs =
                from x in collidersWithParent
                select new PhysicsCollider
                {
                    Value = createBlob( x.collider )
                };
            //var pvs =
            //    from x in collidersWithParent
            //    select new PhysicsVelocity
            //    {

            //    };
            //var pms =
            //    from x in collidersWithParent
            //    select new PhysicsMass
            //    {

            //    };
            //var pds =
            //    from x in collidersWithParent
            //    select new PhysicsGravityFactor
            //    {

            //    };

            var qCompoundCollider =
                from x in collidersWithParent
                group x.collider by x.tfParent
                ;

            foreach( var x in qCompoundCollider )
            {

                if( x.Count > 1 )

            }
                select
                    from c in g
                    let tfParent = g.Key
                    let tfCollider = c.transform
                    let tf = new RigidTransform
                    {
                        pos = tfCollider.position - tfParent.position,
                        rot = tfCollider.rotation * Quaternion.Inverse( tfParent.rotation ),
                    }
                    let blob = x.y
                    select new CompoundCollider.ColliderBlobInstance
                    {
                        Collider = c
                    }

            //var q =
            //    from c in collidersWithParent
            //    join b in qNameAndBone
            //        on c.collider.name equals b.name
            //    select 1;

            BlobAssetReference<Collider> createBlob( UnityEngine.Collider srcCollider )
            {
                switch( srcCollider )
                {
                    case UnityEngine.SphereCollider srcSphere:
                        var geom = new SphereGeometry
                        {
                            Center = srcSphere.center,
                            Radius = srcSphere.radius,
                        };
                        return SphereCollider.Create( geom );
                }
                return BlobAssetReference<Collider>.Null;
            }

            BlobAssetReference<Collider> c
        }
    }
    // 剛体のないコライダは静的として変換する
    // モーションと同名のオブジェクトは、該当するボーンのエンティティにコンポーネントデータを付加する。
    // 剛体のついていないコライダは、一番近い先祖剛体に合成
}
