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

    public class CharactorAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {




        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawResources, PrefabSettingsAuthoring.PrefabCreators creators )
        {

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var (motionPrefab, streamPrefabs) = motionAuthor.Convert( em, creators.Motion );

            var drawAuthor = this.GetComponent<DrawSkinnedMeshAuthoring>();
            var drawPrefab = drawAuthor.Convert( em, creators.Draw, drawResources );

            var boneAuthor = this.GetComponent<BoneAuthoring>();
            var bonePrefabs = boneAuthor.Convert( em, creators.Bones, motionPrefab, streamPrefabs, drawPrefab );

            this.gameObject.SetActive( false );


            var qChildren = Enumerable
                .Empty<Entity>()
                .Append( motionPrefab )
                .Concat( streamPrefabs )
                .Concat( bonePrefabs )
                .Append( drawPrefab )
                ;

            var prefab = creators.Character.CreatePrefab( em, qChildren );

            streamPrefabs.Dispose();
            bonePrefabs.Dispose();

            return prefab;
        }

    }


    public class CharactorPrefabCreator
    {
        
        EntityArchetype charactorPrefabArchetype;



        public CharactorPrefabCreator( EntityManager em )
        {

            this.charactorPrefabArchetype = em.CreateArchetype
            (
                typeof( LinkedEntityGroup ),
                typeof( Prefab )
            );

        }


        public Entity CreatePrefab( EntityManager em, IEnumerable<Entity> children )
        {
            var prefab = em.CreateEntity( this.charactorPrefabArchetype );

            em.SetLinkedEntityGroup( prefab, children );

            return prefab;
        }

    }


}

