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

            var rbTop = this.GetComponentInChildren<Rigidbody>();//( includeInactive: false );



            var qNameAndBone = motionClip.StreamPaths
                .Select( x => System.IO.Path.GetFileName( x ) )
                .Select( ( name, i ) => (name, i: motionClip.IndexMapFbxToMotion[ i ]) )
                .Where( x => x.i != -1 )
                .Select( x => (x.name, ent: bonePrefabs[ x.i ]) )
                .Append( (rbTop.name, ent:posturePrefab) );
            var namesAndBones = qNameAndBone.ToArray();
                
            var qColliderWithParent =
                from collider in this.GetComponentsInChildren<UnityEngine.Collider>()//( includeInactive:false )
            let tfParent = collider.gameObject
                    .AncestorsAndSelf()
                    .Where( anc => anc.GetComponent<Rigidbody>() != null )
                    .First().transform
                select (collider, tfParent)
                ;
            var collidersWithParent = qColliderWithParent.ToArray();
            
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
                        Collider = createColliderBlob_( x ),
                        CompoundFromChild = rtf,
                    }
                select new PhysicsCollider
                {
                    Value = createFromEnumerable_( qBlob )
                };

            var qEntAndComponent =
                from c in (qColliderGroup, qCompounds).Zip()
                join b in namesAndBones
                    on c.x.Key.name equals b.name
                select (b.ent, c:c.y, rb:c.x.Key.GetComponent<Rigidbody>())
                ;

            foreach( var (ent, c, rb) in qEntAndComponent )
            {
                em.AddComponentData( ent, c );

                //if( !rb.isKinematic )
                    addDynamicComponentData_( ent, rb );
            }
            return;

            var qJoint =
                from j in this.GetComponentsInChildren<UnityEngine.Joint>()
                //.Do( x=>Debug.Log(x.name))
                join a in namesAndBones
                    on j.name equals a.name
                join b in namesAndBones
                    on j.connectedBody.name equals b.name
                let jointData = createJointBlob_( j )
                select (a, b, j, jointData)
                //select addJointComponentData_( a.ent, jointData, b.ent, a.ent, j.enableCollision )
                ;
            foreach( var (a, b, j, jointData) in qJoint )
            {
                addJointComponentData_( a.ent, jointData, a.ent, b.ent, j.enableCollision );
            }


            return;


            BlobAssetReference<Collider> createFromEnumerable_
                ( IEnumerable<CompoundCollider.ColliderBlobInstance> src )
            {
                using( var arr = src.ToNativeArray( Allocator.Temp ) )
                    return CompoundCollider.Create( arr );
            }
            
            BlobAssetReference<Collider> createColliderBlob_( UnityEngine.Collider srcCollider )
            {
                switch( srcCollider )
                {
                    case UnityEngine.SphereCollider srcSphere:
                        return srcSphere.ProduceColliderBlob();

                    case UnityEngine.CapsuleCollider srcCapsule:
                        return srcCapsule.ProduceColliderBlob();

                    case UnityEngine.BoxCollider srcBox:
                        return srcBox.ProduceColliderBlob();
                }
                return BlobAssetReference<Collider>.Null;
            }
            
            void addDynamicComponentData_( Entity ent, Rigidbody rb )
            {
                em.AddComponentData( ent, new PhysicsVelocity() );

                var massProp = em.HasComponent<PhysicsCollider>( ent )
                    ? em.GetComponentData<PhysicsCollider>( ent ).MassProperties
                    : MassProperties.UnitSphere;
                em.AddComponentData( ent,
                    rb.isKinematic
                        ? PhysicsMass.CreateKinematic( massProp )
                        : PhysicsMass.CreateDynamic( massProp, rb.mass )
                );

                var freez_xy = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                if( !rb.isKinematic && rb.constraints == freez_xy )
                {
                    var phymass = em.GetComponentData<PhysicsMass>( ent );
                    phymass.InverseInertia = new float3( 0, 1, 0 );
                    em.SetComponentData( ent, phymass );
                }
            }


            BlobAssetReference<JointData> createJointBlob_( UnityEngine.Joint srcJoint )
            {
                switch( srcJoint )
                {
                    case UnityEngine.CharacterJoint srcChJoint:
                        //return JointData.CreateRagdoll
                        //(

                        //);
                        return JointData.CreateBallAndSocket( srcChJoint.anchor, srcChJoint.connectedAnchor );
                        //return JointData.CreateFixed( srcChJoint.anchor, srcChJoint.connectedAnchor, quaternion.identity, quaternion.identity );
                }
                return BlobAssetReference<JointData>.Null;
            }

            //unsafe Entity createJoint_
            unsafe void addJointComponentData_
                ( Entity jointEntity, BlobAssetReference<JointData> jointData, Entity entityA, Entity entityB, bool isEnableCollision = false )
            {
                //Entity jointEntity2 = em.CreateEntity();// typeof( PhysicsJoint ) );
                em.AddComponentData( jointEntity, 
                    new PhysicsJoint
                    {
                        JointData = jointData,
                        EntityA = entityA,
                        EntityB = entityB,
                        EnableCollision = ( isEnableCollision ? 1 : 0 )
                    } 
                );
                //return jointEntity;
            }
            
        }
    }
    // 剛体のないコライダは静的として変換する
    // モーションと同名のオブジェクトは、該当するボーンのエンティティにコンポーネントデータを付加する。
    // 剛体のついていないコライダは、一番近い先祖剛体に合成






    public static class LegacyColliderProducer
    {
        static public BlobAssetReference<Collider> ProduceColliderBlob( this UnityEngine.BoxCollider shape )
        {
            var worldCenter = math.mul( shape.transform.localToWorldMatrix, new float4( shape.center, 1f ) );
            var shapeFromWorld = math.inverse(
                new float4x4( new RigidTransform( shape.transform.rotation, shape.transform.position ) )
            );

            var geometry = new BoxGeometry
            {
                Center = math.mul( shapeFromWorld, worldCenter ).xyz,
                Orientation = quaternion.identity
            };

            var linearScale = (float3)shape.transform.lossyScale;
            geometry.Size = math.abs( shape.size * linearScale );

            geometry.BevelRadius = math.min( ConvexHullGenerationParameters.Default.BevelRadius, math.cmin( geometry.Size ) * 0.5f );

            return BoxCollider.Create(
                geometry//,
                //ProduceCollisionFilter( shape ),
                //ProduceMaterial( shape )
            );
        }

        static public BlobAssetReference<Collider> ProduceColliderBlob( this UnityEngine.CapsuleCollider shape )
        {
            var linearScale = (float3)shape.transform.lossyScale;

            // radius is max of the two non-height axes
            var radius = shape.radius * math.cmax( new float3( math.abs( linearScale ) ) { [ shape.direction ] = 0f } );

            var ax = new float3 { [ shape.direction ] = 1f };
            var vertex = ax * ( 0.5f * shape.height );
            var rt = new RigidTransform( shape.transform.rotation, shape.transform.position );
            var worldCenter = math.mul( shape.transform.localToWorldMatrix, new float4( shape.center, 0f ) );
            var offset = math.mul( math.inverse( new float4x4( rt ) ), worldCenter ).xyz - shape.center * math.abs( linearScale );

            var v0 = offset + ( (float3)shape.center + vertex ) * math.abs( linearScale ) - ax * radius;
            var v1 = offset + ( (float3)shape.center - vertex ) * math.abs( linearScale ) + ax * radius;

            return CapsuleCollider.Create(
                new CapsuleGeometry { Vertex0 = v0, Vertex1 = v1, Radius = radius }//,
                //ProduceCollisionFilter( shape ),
                //ProduceMaterial( shape )
            );
        }

        static public BlobAssetReference<Collider> ProduceColliderBlob( this UnityEngine.SphereCollider shape )
        {
            var worldCenter = math.mul( shape.transform.localToWorldMatrix, new float4( shape.center, 1f ) );
            var shapeFromWorld = math.inverse(
                new float4x4( new RigidTransform( shape.transform.rotation, shape.transform.position ) )
            );
            var center = math.mul( shapeFromWorld, worldCenter ).xyz;

            var linearScale = (float3)shape.transform.lossyScale;
            var radius = shape.radius * math.cmax( math.abs( linearScale ) );

            return SphereCollider.Create(
                new SphereGeometry { Center = center, Radius = radius }//,
                //ProduceCollisionFilter( shape ),
                //ProduceMaterial( shape )
            );
        }
        
    }



}
