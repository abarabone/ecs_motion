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
            ( EntityManager em, Entity posturePrefab, NativeArray<Entity> bonePrefabs )
        {

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//


            var rb = this.GetComponentInChildren<Rigidbody>();

            //em.AddComponentData( posturePrefab,
            //    new PhysicsCollider
            //    {
            //        Value = createBlob_( x.collider )
            //    }
            //);
            em.AddComponentData( posturePrefab, new PhysicsVelocity() );
            //em.AddComponentData( posturePrefab,
            //    new PhysicsMass
            //    {
            //        CenterOfMass = rb.centerOfMass,
            //        InertiaOrientation = rb.inertiaTensor,
            //    }
            //);
            em.AddComponentData( posturePrefab, new PhysicsGravityFactor { Value = 1.0f } );



            var qNameAndBone = motionClip.StreamPaths
                .Select( x => System.IO.Path.GetFileName( x ) )
                .Select( ( name, i ) => (name, i: motionClip.IndexMapFbxToMotion[ i ]) )
                .Where( x => x.i != -1 )
                .Select( x => (x.name, ent: bonePrefabs[ x.i ]) );
                
            var qColliderwithParent =
                from collider in this.GetComponentsInChildren<UnityEngine.Collider>()
                let tfParent = collider.gameObject
                    .AncestorsAndSelf()
                    .Where( anc => anc.GetComponent<Rigidbody>() != null )
                    .First().transform
                select (collider, tfParent)
                ;
            var collidersWithParent = qColliderwithParent.ToArray();

            //var blobRefs =
            //    from x in collidersWithParent
            //    select new PhysicsCollider
            //    {
            //        Value = createBlob_( x.collider )
            //    };
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

            var qColliderGroup =
                from x in collidersWithParent
                group x.collider by x.tfParent
                ;

            var qCompounds =
                from g in qColliderGroup
                let qBlob =
                    from x in g
                    let tfParent = g.Key
                    let tfCollider = x.transform
                    let rtf = new RigidTransform
                    {
                        pos = tfCollider.position - tfParent.position,
                        rot = tfCollider.rotation * Quaternion.Inverse( tfParent.rotation ),
                    }
                    select new CompoundCollider.ColliderBlobInstance
                    {
                        Collider = createBlob_( x ),
                        CompoundFromChild = rtf,
                    }
                select new PhysicsCollider
                {
                    Value = createFromEnumerable_( qBlob )
                };

            var qEntAndComponent =
                from c in (qColliderGroup, qCompounds).Zip()
                join b in qNameAndBone
                    on c.x.Key.name equals b.name
                select (b.ent, c:c.y)
                ;

            foreach( var x in qEntAndComponent )
            {
                em.AddComponentData( x.ent, x.c );
            }

            return;


            BlobAssetReference<Collider> createFromEnumerable_( IEnumerable<CompoundCollider.ColliderBlobInstance> src )
            {
                using( var arr = src.ToNativeArray( Allocator.Temp ) )
                    return CompoundCollider.Create( arr );
            }
            
            BlobAssetReference<Collider> createBlob_( UnityEngine.Collider srcCollider )
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
            
        }
    }
    // 剛体のないコライダは静的として変換する
    // モーションと同名のオブジェクトは、該当するボーンのエンティティにコンポーネントデータを付加する。
    // 剛体のついていないコライダは、一番近い先祖剛体に合成


}
