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




        public override Entity Convert( EntityManager em, PrefabSettingsAuthoring.PrefabCreators creators )
        {

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var motionPrefab = motionAuthor.Convert( em, creators );

            return creators.Character.CreatePrefab( motionPrefab );
        }

    }


    public class CharactorPrefabCreator
    {


        EntityManager em;

        EntityArchetype charactorPrefabArchetype;



        public CharactorPrefabCreator( EntityManager entityManager )
        {

            this.em = entityManager;


            this.charactorPrefabArchetype = this.em.CreateArchetype
            (
                typeof( LinkedEntityGroup ),
                typeof( Prefab )
            );

        }


        public Entity CreatePrefab( Entity motionPrefab )
        {

            var chArchetype = this.charactorPrefabArchetype;

            var prefab = this.em.CreateEntity( chArchetype );
            var links = em.GetBuffer<LinkedEntityGroup>( prefab );
            
            links.Add( new LinkedEntityGroup { Value = prefab } );
            links.Add( new LinkedEntityGroup { Value = motionPrefab } );

            return prefab;
        }

    }

}

