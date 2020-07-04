using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

namespace Abarabone.Authoring
{

    using Abarabone.Character;
    using Abarabone.Geometry;
    using Abarabone.Utilities;
    using Abarabone.Misc;
    using Abarabone.CharacterMotion;
    using Abarabone.Draw;
    using Abarabone.Common.Extension;
    using Abarabone.Model;



    [DisallowMultipleComponent]
    public class BoneLvAuthoring : MonoBehaviour
    {

        public MotionTargetUnit[] Motions;

        public bool UsePhysics;


        public (NameAndEntity[] bonePrefabs, Entity posturePrefab) Convert
            ( EntityManager em, NameAndEntity[] posStreamPrefabs, NameAndEntity[] rotStreamPrefabs, Entity drawPrefab )
        {

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//

            //return Bone.LvPrefabCreator.CreatePrefabs( em, streamPrefabs, drawPrefab, motionClip );
            return (new NameAndEntity[] { }, Entity.Null);
        }
    }


    static public class BoneLvPrefabCreator
    {

        static EntityArchetypeCache postureArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                //typeof( Posture.NeedTransformTag ),
                //typeof( Posture.LinkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( Prefab )
            )
        );

        static EntityArchetypeCache boneArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                //typeof( Bone.RelationLinkData ),
                typeof( DrawTransform.LinkData ),
                typeof( Bone.Stream0LinkData ),
                typeof( DrawTransform.IndexData ),
                typeof( DrawTransform.TargetWorkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( Prefab )
            )
        );


        static public (NativeArray<Entity> bonePrefabs, Entity posturePrefab) CreatePrefabs
            ( EntityManager em, NativeArray<Entity> streamPrefabs, Entity drawPrefab, MotionClip motionClip )
        {
            var postArchetype = postureArchetypeCache.GetOrCreateArchetype( em );
            var boneArchetype = boneArchetypeCache.GetOrCreateArchetype( em );

            var posturePrefab = em.CreateEntity( postArchetype );

            
            var bonePrefabs = createBonePrefabs( em, motionClip, boneArchetype );
            setBoneId( em, bonePrefabs, bonePrefabs.Length );
            setStreamLinks( em, bonePrefabs, streamPrefabs, motionClip );
            setDrawLinks( em, bonePrefabs, drawPrefab, motionClip );
            setBoneRelationLinks( em, bonePrefabs, posturePrefab, motionClip );

            
            em.SetComponentData( posturePrefab, new Rotation { Value = quaternion.identity } );

            return (bonePrefabs, posturePrefab);


            NativeArray<Entity> createBonePrefabs
                ( EntityManager em_, MotionClip motionClip_, EntityArchetype archetype )
            {
                var boneLength = motionClip_.StreamPaths.Length;
                var bonePrefabs_ = new NativeArray<Entity>( boneLength, Allocator.Temp );

                em_.CreateEntity( archetype, bonePrefabs_ );

                return bonePrefabs_;
            }

            void setBoneId( EntityManager em_, NativeArray<Entity> bonePreafabs_, int boneLength )
            {
                em_.SetComponentData( bonePreafabs_,
                    from x in Enumerable.Range( 0, bonePreafabs_.Length )
                    select new DrawTransform.IndexData { BoneLength = boneLength, BoneId = x }
                );
            }

            void setStreamLinks(
                EntityManager em_, NativeArray<Entity> bonePrefabs_, NativeArray<Entity> streamPrefabs_,
                MotionClip motionClip_
            )
            {
                var boneLength = motionClip_.StreamPaths.Length;

                var qPosStreams = streamPrefabs_
                    .Take( boneLength );
                var qRotStreams = streamPrefabs_
                    .Skip( boneLength )
                    .Take( boneLength );
                var qStreamlinkers =
                    from ents in (qPosStreams, qRotStreams).Zip()
                    select new Bone.Stream0LinkData
                    {
                        PositionStreamEntity = ents.x,
                        RotationStreamEntity = ents.y,
                    };

                em_.SetComponentData( bonePrefabs_, qStreamlinkers );
            }

            void setDrawLinks(
                EntityManager em_,
                NativeArray<Entity> bonePrefabs_, Entity drawPrefab_, MotionClip motionClip_
            )
            {
                var boneLength = motionClip_.StreamPaths.Length;

                var qDrawLinker = Enumerable
                    .Repeat( new DrawTransform.LinkData { DrawInstanceEntity = drawPrefab_ }, boneLength );

                em_.SetComponentData( bonePrefabs_, qDrawLinker );
            }

            unsafe void setBoneRelationLinks
                ( EntityManager em_, NativeArray<Entity> bonePrefabs_, Entity posturePrefab_, MotionClip motionClip_ )
            {
                var qParentIds = motionClip_.StreamPaths
                    //.QueryParentIdList();
                    .Select( x => 0 );// まにあわせ

                var qEnts = bonePrefabs_
                    .Prepend( posturePrefab_ )
                    .Append( Entity.Null );

                var qPathDepthCounts =
                    from path in motionClip_.StreamPaths
                    select path.Where( x => x == '/' ).Count() + 1
                    ;

                using( var parentIds = qParentIds.ToNativeArray( Allocator.Temp ) )
                using( var boneEnts = qEnts.ToNativeArray( Allocator.Temp ) )
                using( var boneDepthLevels = qPathDepthCounts.ToNativeArray(Allocator.Temp) )
                {
                    foreach( var i in Enumerable.Range( 0, motionClip.StreamPaths.Length ) )
                    {
                        var ent = boneEnts[ i + 1 ];
                        var parent = boneEnts[ parentIds[ i ] + 1 ];
                        var lv = boneDepthLevels[ i ];
                        setLvLinker( ent, parent, lv );
                    }
                }

                void setLvLinker( Entity ent, Entity parent, int lv )
                {
                    switch( lv )
                    {
                        case 1: em_.AddComponentData( ent, new Bone.Lv01LinkData { ParentBoneEntity = parent } ); break;
                        case 2: em_.AddComponentData( ent, new Bone.Lv02LinkData { ParentBoneEntity = parent } ); break;
                        case 3: em_.AddComponentData( ent, new Bone.Lv03LinkData { ParentBoneEntity = parent } ); break;
                        case 4: em_.AddComponentData( ent, new Bone.Lv04LinkData { ParentBoneEntity = parent } ); break;
                        case 5: em_.AddComponentData( ent, new Bone.Lv05LinkData { ParentBoneEntity = parent } ); break;
                        case 6: em_.AddComponentData( ent, new Bone.Lv06LinkData { ParentBoneEntity = parent } ); break;
                        case 7: em_.AddComponentData( ent, new Bone.Lv07LinkData { ParentBoneEntity = parent } ); break;
                        case 8: em_.AddComponentData( ent, new Bone.Lv08LinkData { ParentBoneEntity = parent } ); break;
                        case 9: em_.AddComponentData( ent, new Bone.Lv09LinkData { ParentBoneEntity = parent } ); break;
                    }
                }
            }

        }

    }
}