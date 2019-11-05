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
        
        // 生成されたジョイントエンティティを返す。
        public NativeArray<Entity> Convert
            ( EntityManager em, Entity posturePrefab, NativeArray<Entity> bonePrefabs )
        {

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//

            var rbTop = this.GetComponentInChildren<Rigidbody>();//

            var srcColliders = this.GetComponentsInChildren<UnityEngine.Collider>();


            // 名前とボーンエンティティの組を配列化
            var qNameAndBone = motionClip.StreamPaths
                .Select( x => System.IO.Path.GetFileName( x ) )
                .Select( ( name, i ) => (name, i: motionClip.IndexMapFbxToMotion[ i ]) )
                .Where( x => x.i != -1 )
                .Select( x => (x.name, ent: bonePrefabs[ x.i ]) )
                .Append( (rbTop.name, ent:posturePrefab) );
            var namesAndBones = qNameAndBone.ToArray();


            // クエリ用コライダの生成
            // ・マテリアルが "xxx overlap collider" という名前になっているものを抽出
            // ・対応するボーンエンティティに専用のコンポーネントデータを付加
            var qQueryableCollider =
                from x in srcColliders
                where x.sharedMaterial != null
                where x.sharedMaterial.name.StartsWith("overlap ")
                join bone in qNameAndBone
                    on x.name equals bone.name
                select (bone.ent, c:x)
                ;
            foreach( var (ent, c) in qQueryableCollider )
            {
                addQuearyableColliderBlobs_( ent, c, 0 );
            }


            // コライダとそれが付くべき剛体の組を配列化（同じ剛体に複数のコライダもあり）
            // ・有効でないコライダは除外する
            var qColliderWithParent =
                from collider in srcColliders
                where collider.enabled
                let parent = collider.gameObject
                    .AncestorsAndSelf()
                    .Where( anc => anc.GetComponent<Rigidbody>() != null )
                    .First()
                select (collider, parent)
                ;
            var collidersWithParent = qColliderWithParent.ToArray();
            
            // 同じ剛体を親とするコライダをグループ化するクエリ
            var qColliderGroup =
                from x in collidersWithParent
                group x.collider by x.parent
                ;

            // 剛体を持たない子コライダを合成して、コライダコンポーネントデータを生成するクエリ
            var qCompounds =
                from g in qColliderGroup
                select new PhysicsCollider
                {
                    Value = createBlobCollider_( srcColliders_: g, parent: g.Key, groupIndex: 0 ),
                };

            // コライダがついているオブジェクトに相当するボーンのエンティティに、コライダコンポーネントデータを付加
            // ・コライダ不要なら、質量プロパティだけ生成してコライダはつけないようにしたい（未実装）
            var qEntAndComponent =
                from c in (qColliderGroup, qCompounds).Zip()
                join b in namesAndBones
                    on c.x.Key.name equals b.name
                select (b.ent, c:c.y)
                ;
            foreach( var (ent, c) in qEntAndComponent )
            {
                em.AddComponentData( ent, c );
            }

            // 剛体がついているオブジェクトに相当するボーンのエンティティに、各種コンポーネントデータを付加
            // ・キネマティックは質量ゼロにするが、速度や質量プロパティは付加する。
            // ・コライダがない場合は、球の質量プロパティになる。
            var qRbAndBone =
                from x in this.GetComponentsInChildren<Rigidbody>()
                join b in namesAndBones
                    on x.name equals b.name
                select (rb: x, b.ent)
                ;
            foreach( var (rb, ent) in qRbAndBone )
            {
                addDynamicComponentData_ByRigidbody_( ent, rb, posturePrefab );
            }
            //return new NativeArray<Entity>(0,Allocator.Temp);


            // ジョイントの生成。両端のオブジェクトに相当するエンティティを特定する。
            // ・ジョイントはエンティティとして生成する。
            // 　（コライダと同じエンティティに着けても動作したが、サンプルではこうしている）
            // 　（また、ラグドールジョイントはなぜか２つジョイントを返してくるので、同じエンティティには付けられない）
            var qJoint =
                from j in this.GetComponentsInChildren<UnityEngine.Joint>()
                //.Do( x=>Debug.Log(x.name))
                join a in namesAndBones
                    on j.name equals a.name
                join b in namesAndBones
                    on j.connectedBody.name equals b.name
                let jointData = createJointBlob_( j )
                //select (a, b, j, jointData)
                select addJointComponentData_( a.ent, jointData, a.ent, b.ent, j.enableCollision )
                ;
            return qJoint.SelectMany().ToNativeArray( Allocator.Temp );



            void addQuearyableColliderBlobs_
                ( Entity ent, UnityEngine.Collider srcCollider, int groupIndex )
            {
                var blob = createBlobCollider_( new[] { srcCollider }, srcCollider.gameObject, groupIndex );

                switch( srcCollider.sharedMaterial.name )
                {
                    case "overlap ground":
                        em.AddComponentData( ent, new GroundHitColliderData { Collider = blob } );
                        break;
                }
            }

            BlobAssetReference<Collider> compoundColliderBlobsFromEnumerable_
                ( IEnumerable<CompoundCollider.ColliderBlobInstance> src )
            {
                using( var arr = src.ToNativeArray( Allocator.Temp ) )
                    return CompoundCollider.Create( arr );
            }

            BlobAssetReference<Collider> createBlobCollider_
                ( IEnumerable<UnityEngine.Collider> srcColliders_, GameObject parent, int groupIndex )
            {
                return ( srcColliders_.Count() > 1 || srcColliders_.First().gameObject != parent )
                    ? queryBlobInstances_( srcColliders_, parent.transform, groupIndex )
                        .To( compoundColliderBlobsFromEnumerable_ )
                    : createColliderBlob_( srcColliders_.First(), groupIndex )
                    ;

                IEnumerable<CompoundCollider.ColliderBlobInstance> queryBlobInstances_
                    ( IEnumerable<UnityEngine.Collider> srcColliders__, Transform tfParent, int groupIndex_ )
                {
                    return
                        from x in srcColliders__
                        let tfCollider = x.transform
                        let rtf = new RigidTransform
                        {
                            pos = tfCollider.position - tfParent.position,
                            rot = tfCollider.rotation * Quaternion.Inverse( tfParent.rotation ),
                        }
                        select new CompoundCollider.ColliderBlobInstance
                        {
                            Collider = createColliderBlob_( x, groupIndex_ ),
                            CompoundFromChild = rtf,
                        };
                }

            }

            BlobAssetReference<Collider> createColliderBlob_( UnityEngine.Collider srcCollider, int groupIndex )
            {
                switch( srcCollider )
                {
                    case UnityEngine.SphereCollider srcSphere:
                        return srcSphere.ProduceColliderBlob( groupIndex );

                    case UnityEngine.CapsuleCollider srcCapsule:
                        return srcCapsule.ProduceColliderBlob( groupIndex );

                    case UnityEngine.BoxCollider srcBox:
                        return srcBox.ProduceColliderBlob( groupIndex );
                }
                return BlobAssetReference<Collider>.Null;
            }
            
            void addDynamicComponentData_ByRigidbody_( Entity ent, Rigidbody rb, Entity postureEnt )
            {
                var massProp = em.HasComponent<PhysicsCollider>( ent )
                    ? em.GetComponentData<PhysicsCollider>( ent ).MassProperties
                    : MassProperties.UnitSphere;

                var phymass = rb.isKinematic
                    ? PhysicsMass.CreateKinematic( massProp )
                    : PhysicsMass.CreateDynamic( massProp, rb.mass );

                // ＸＹ回転拘束だけ特例で設定する
                var freez_xy = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                if( rb.constraints == freez_xy )
                {
                    phymass.InverseInertia = new float3( 0, 1, 0 );
                }

                //if( !rb.isKinematic )
                    em.AddComponentData( ent, phymass );
                // キネマティックの場合は、つけなくても大丈夫みたい（主にジョイントにとって）
                // が、いちおうつけておく

                //if( !rb.isKinematic )
                    em.AddComponentData( ent, new PhysicsVelocity() );
                // ジョイント付けると、質量ゼロにしても速度が必要みたい（ないと荒ぶる）
                // もしくは、コライダがあると安定したように見えた

                if( rb.isKinematic || !rb.useGravity )
                    em.AddComponentData( ent, new PhysicsGravityFactor { Value = 0.0f } );
                // 質量ゼロにしても、なぜか重力の影響を受け続けるのでオフる

                //if( !rb.isKinematic )
                {
                    em.AddComponentData( ent, new BoneInitializeData { PostureEntity = postureEnt } );
                    em.SetComponentData( ent, new Translation { Value = rb.position } );
                    em.SetComponentData( ent, new Rotation { Value = rb.rotation } );
                }
            }


            BlobAssetReference<JointData>[] createJointBlob_( UnityEngine.Joint srcJoint )
            {
                switch( srcJoint )
                {
                    case UnityEngine.CharacterJoint srcChJoint when srcChJoint.GetComponent<RagdollJointAuthoring>() != null:
                    {
                        var srcRagdollJoint = srcChJoint.GetComponent<RagdollJointAuthoring>();
                        JointData.CreateRagdoll
                        (
                            srcRagdollJoint.positionAinA,
                            srcRagdollJoint.positionBinB,
                            srcRagdollJoint.twistAxisInA,
                            srcRagdollJoint.twistAxisInB,
                            srcRagdollJoint.perpendicularAxisInA,
                            srcRagdollJoint.perpendicularAxisInB,
                            math.radians( srcRagdollJoint.maxConeAngle ),
                            math.radians( srcRagdollJoint.minPerpendicularAngle ),
                            math.radians( srcRagdollJoint.maxPerpendicularAngle ),
                            math.radians( srcRagdollJoint.minTwistAngle ),
                            math.radians( srcRagdollJoint.maxTwistAngle ),
                            out var jointData0,
                            out var jointData1
                        );
                        return new[] { jointData0, jointData1 };
                    }

                    case UnityEngine.CharacterJoint srcChJoint2:
                    {
                        var blob = JointData.CreateBallAndSocket( srcChJoint2.anchor, srcChJoint2.connectedAnchor );
                        return new[] { blob };
                    }
                }
                return new BlobAssetReference<JointData>[] {};
            }

            //unsafe Entity createJoint_
            unsafe Entity[] addJointComponentData_(
                Entity jointEntity,
                BlobAssetReference<JointData>[] jointDataArray,
                Entity entityA, Entity entityB,
                bool isEnableCollision = false
            )
            {

                return ( from x in jointDataArray select createJointEntity_( x ) ).ToArray();
                

                Entity createJointEntity_( BlobAssetReference<JointData> jd )
                {
                    var ent = em.CreateEntity( typeof( Prefab ), typeof( PhysicsJoint ) );
                    em.SetComponentData( ent,
                        new PhysicsJoint
                        {
                            JointData = jd,
                            EntityA = entityA,
                            EntityB = entityB,
                            EnableCollision = ( isEnableCollision ? 1 : 0 )
                        }
                    );
                    return ent;
                }
            }
            
        }
    }
    // 剛体のないコライダは静的として変換する
    // モーションと同名のオブジェクトは、該当するボーンのエンティティにコンポーネントデータを付加する。
    // 剛体のついていないコライダは、一番近い先祖剛体に合成






    public static class LegacyColliderProducer
    {

        static CollisionFilter getFilter( UnityEngine.Collider shape, int groupIndex )
        {
            var filter = CollisionFilter.Default;
            filter.GroupIndex = groupIndex;

            var cfa = shape.GetComponentInParent<ColliderFilterAuthoring>();
            if( cfa == null ) return filter;
            
            filter.BelongsTo = cfa.BelongsTo.Value;
            filter.CollidesWith = cfa.CollidesWith.Value;
            return filter;
        }


        static public BlobAssetReference<Collider> ProduceColliderBlob
            ( this UnityEngine.BoxCollider shape, int groupIndex )
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
                geometry, getFilter(shape, groupIndex )
                //ProduceCollisionFilter( shape ),
                //ProduceMaterial( shape )
            );
        }

        static public BlobAssetReference<Collider> ProduceColliderBlob
            ( this UnityEngine.CapsuleCollider shape, int groupIndex )
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
                new CapsuleGeometry { Vertex0 = v0, Vertex1 = v1, Radius = radius }, getFilter( shape, groupIndex )
            //ProduceCollisionFilter( shape ),
            //ProduceMaterial( shape )
            );
        }

        static public BlobAssetReference<Collider> ProduceColliderBlob
            ( this UnityEngine.SphereCollider shape, int groupIndex )
        {
            var worldCenter = math.mul( shape.transform.localToWorldMatrix, new float4( shape.center, 1f ) );
            var shapeFromWorld = math.inverse(
                new float4x4( new RigidTransform( shape.transform.rotation, shape.transform.position ) )
            );
            var center = math.mul( shapeFromWorld, worldCenter ).xyz;

            var linearScale = (float3)shape.transform.lossyScale;
            var radius = shape.radius * math.cmax( math.abs( linearScale ) );

            return SphereCollider.Create(
                new SphereGeometry { Center = center, Radius = radius }, getFilter(shape, groupIndex )
                //ProduceCollisionFilter( shape ),
                //ProduceMaterial( shape )
            );
        }
        
    }



}
