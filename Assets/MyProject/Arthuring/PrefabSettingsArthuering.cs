using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Charactor;

namespace Abss.Arthuring
{

    public struct PrefabUnit : IDisposable
    {
        public Entity Prefab;

        public MotionPrefabUnit Motion;


        public void Dispose()
        {
            this.Motion.Dispose();
        }
    }


    public class PrefabSettingsAuthoring : MonoBehaviour
    {

        public GameObject[] Prefabs;


        [HideInInspector]
        public PrefabUnit[] PrefabEntities;


        MotionPrefabCreator motionPrefabCreator;


        void Awake()
        {
            var em = World.Active.EntityManager;

            this.motionPrefabCreator = new MotionPrefabCreator( em );


            foreach( var prefab in this.Prefabs )
            {
                prefab.
            }
        }

        void OnDestroy()
        {
            foreach( var unit in this.PrefabEntities )
                unit.Dispose();
        }


        public interface IConvertToPrefab
        {
            Entity Convert( PrefabSettingsAuthoring arthur );
        }
    }
}

