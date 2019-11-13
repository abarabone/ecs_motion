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

using Abss.Character;
using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Common.Extension;

namespace Abss.Arthuring
{
    
    public class BoneLvAuthoring : MonoBehaviour, CharacterAuthoring.IBoneConverter
    {

        public MotionTargetUnit[] Motions;

        public bool UsePhysics;


        public (NativeArray<Entity> bonePrefabs, Entity posturePrefab) Convert
            ( EntityManager em, IEnumerable<NameAndEntity> streamPrefabs, Entity drawPrefab )
        {

            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//

            //return BoneLvPrefabCreator.CreatePrefabs( em, streamPrefabs, drawPrefab, motionClip );
            return (new NativeArray<Entity>(), Entity.Null);
        }
    }


    static public class BoneLvPrefabCreator
    {

        static EntityArchetypeCache postureArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                //typeof( PostureNeedTransformTag ),
                //typeof( PostureLinkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( Prefab )
            )
        );

        static EntityArchetypeCache boneArchetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                //typeof( BoneRelationLinkData ),
                typeof( BoneDrawLinkData ),
                typeof( BoneStreamLinkData ),
                typeof( BoneIndexData ),
                typeof( BoneDrawTargetIndexWorkData ),
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
            setBoneId( em, bonePrefabs, drawPrefab );
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

            void setBoneId( EntityManager em_, NativeArray<Entity> bonePreafabs_, Entity drawPrefab_ )
            {
                var draw = em_.GetComponentData<DrawModelIndexData>( drawPrefab_ );

                em_.SetComponentData( bonePreafabs_,
                    from x in Enumerable.Range( 0, bonePreafabs_.Length )
                    select new BoneIndexData { ModelIndex = draw.ModelIndex, BoneId = x }
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
                    select new BoneStreamLinkData
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
                    .Repeat( new BoneDrawLinkData { DrawEntity = drawPrefab_ }, boneLength );

                em_.SetComponentData( bonePrefabs_, qDrawLinker );
            }

            unsafe void setBoneRelationLinks
                ( EntityManager em_, NativeArray<Entity> bonePrefabs_, Entity posturePrefab_, MotionClip motionClip_ )
            {
                var qParentIds = motionClip_.StreamPaths
                    .QueryParentIdList();

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
                        case 1: em_.AddComponentData( ent, new BoneLv01LinkData { ParentBoneEntity = parent } ); break;
                        case 2: em_.AddComponentData( ent, new BoneLv02LinkData { ParentBoneEntity = parent } ); break;
                        case 3: em_.AddComponentData( ent, new BoneLv03LinkData { ParentBoneEntity = parent } ); break;
                        case 4: em_.AddComponentData( ent, new BoneLv04LinkData { ParentBoneEntity = parent } ); break;
                        case 5: em_.AddComponentData( ent, new BoneLv05LinkData { ParentBoneEntity = parent } ); break;
                        case 6: em_.AddComponentData( ent, new BoneLv06LinkData { ParentBoneEntity = parent } ); break;
                        case 7: em_.AddComponentData( ent, new BoneLv07LinkData { ParentBoneEntity = parent } ); break;
                        case 8: em_.AddComponentData( ent, new BoneLv08LinkData { ParentBoneEntity = parent } ); break;
                        case 9: em_.AddComponentData( ent, new BoneLv09LinkData { ParentBoneEntity = parent } ); break;
                    }
                }
            }

        }

    }
}