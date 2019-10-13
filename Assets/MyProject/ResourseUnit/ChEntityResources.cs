using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Arthuring;
using Abss.Motion;
using Abss.Cs;

namespace Abss.Charactor
{
    
    /*
    public struct CharactorPrefabUnit
    {
        public Entity Prefab;
    }
    


    class CharactorPrefabHolder : IDisposable
    {

        public MotionPrefabUnit[] MotionPrefabResources { get; private set; }
        


        public CharactorPrefabHolder( EntityManager em, CharactorResourceUnit[] resources )
        {

            var archetype = createArchetype( em );

            this.MotionPrefabResources = queryPrefabUnits(resources,archetype).ToArray();

            return;

            
            EntityArchetype createArchetype( EntityManager em_ ) =>
                em_.CreateArchetype
                (
                    typeof( LinkedEntityGroup )
                    
                );
            
            IEnumerable<MotionPrefabUnit> queryPrefabUnits
                ( CharactorResourceUnit[] resources_, EntityArchetype archetype_ )
            {
                return null;
            }

            Entity createPrefab
                ( EntityManager em_, BlobAssetReference<MotionBlobData> motionClipData, EntityArchetype archetype_ )
            {

                return Entity.Null;
            }
        }

        public void Dispose()
        {
            //this.motionPrefabDatas.Do( x => x.Dispose() );// .Do() が機能しない？？
            foreach( var x in this.MotionPrefabResources )
                x.Dispose();
        }
            
    }
           */
}