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

namespace Abss.Arthuring
{

    public class CharactorAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        void Convert( NativeList<Entity> dstPrefabs, )


        public override Entity Convert
            ( EntityManager em, DrawMeshResourceHolder drawres, PrefabSettingsAuthoring.PrefabCreators creators )
        {

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var motionPrefab = motionAuthor.Convert( em, drawres, creators );

            var drawAuthor = this.GetComponent<DrawSkinnedMeshAuthoring>();
            var drawPrefab = drawAuthor.Convert( em, drawres, creators );
            
            this.gameObject.SetActive( false );

            return creators.Character.CreatePrefab( em, motionPrefab, drawPrefab );
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


        public Entity CreatePrefab( EntityManager em, Entity motionPrefab, Entity drawPrefab )
        {

            var chArchetype = this.charactorPrefabArchetype;

            var prefab = em.CreateEntity( chArchetype );
            var links = em.GetBuffer<LinkedEntityGroup>( prefab );
            
            links.Add( new LinkedEntityGroup { Value = prefab } );
            links.Add( new LinkedEntityGroup { Value = drawPrefab } );
            links.Add( new LinkedEntityGroup { Value = motionPrefab } );

            return prefab;
        }

    }


}

