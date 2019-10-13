using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Charactor;
using Abss.Common.Extension;

namespace Abss.Arthuring
{

    public struct MotionTargetUnit
    {
        public MotionAuthoring Motion;
        public float Weight;
    }

    public class BoneAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        public MotionTargetUnit[] Motions;

        public bool UsePhysics;


        public override Entity Convert( EntityManager em, DrawMeshResourceHolder drawres, PrefabSettingsAuthoring.PrefabCreators creators )
        {



        }
    }


    public class BonePrefabCreator
    {

        EntityArchetype TransformingPrefabArchetype;
        EntityArchetype BonePrefabArchetype;


        public BonePrefabCreator( EntityManager em )
        {

            this.TransformingPrefabArchetype = em.CreateArchetype
            (
                typeof(LinkedEntityGroup),
                typeof(Prefab)
            );

            this.BonePrefabArchetype = em.CreateArchetype
            (
                typeof( BoneDrawTargetIndexData ),
                typeof( BoneDrawLinkData ),
                typeof( BoneTransformLinkData ),
                typeof( BoneStreamLinkData ),
                typeof( Translation ),
                typeof( Rotation ),
                typeof( Prefab )
            );

        }


        public Entity CreatePrefab( EntityManager em, Entity motionPrefab, Entity drawPrefab )
        {

            var prefab = em.CreateEntity( this.TransformingPrefabArchetype );

            var bonePrefabs = createBonePrefabs( em, motionPrefab, drawPrefab );

            em.SetLinkedEntityGroup( prefab, bonePrefabs );

            bonePrefabs.Dispose();

            return prefab;

            

            NativeArray<Entity> createBonePrefabs
                ( EntityManager em_, Entity motionPrefab_, Entity drawPrefab_ )
            {
                var motionLinkers = em_.GetBuffer<LinkedEntityGroup>( motionPrefab_ );
                var motionData = em_.GetComponentData<MotionClipData>( motionPrefab_ );
                
                ref var motionClip = ref motionData.ClipData.Value;
                var boneLength = motionClip.BoneParents.Length;


                var bonePrefabs_ = new NativeArray<Entity>( boneLength, Allocator.Temp );
                em_.CreateEntity( this.BonePrefabArchetype, bonePrefabs_ );
                

                var qPosStreams = motionLinkers
                    .Skip( 1 )
                    .Select( x => x.Value );
                var qRotStreams = motionLinkers
                    .Skip( 1 + boneLength )
                    .Select( x => x.Value );
                var qStreamlinkers =
                    from ents in (qPosStreams, qRotStreams).Zip()
                    select new BoneStreamLinkData
                    {
                        PositionStreamEntity = ents.x,
                        RotationStreamEntity = ents.y,
                    };
                em.SetComponentData( bonePrefabs_, qStreamlinkers );


                var qDrawLinker = Enumerable
                    .Repeat( new BoneDrawLinkData { DrawEntity = drawPrefab }, boneLength );
                em.SetComponentData( bonePrefabs_, qDrawLinker );


                var qBoneLinker = 

                return bonePrefabs_;
            }
        }

    }

}