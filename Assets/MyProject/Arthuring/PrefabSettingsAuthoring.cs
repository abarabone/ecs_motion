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

    public class PrefabSettingsAuthoring : MonoBehaviour
    {

        public IConvertToPrefab[] Prefabs;


        [HideInInspector]
        public PrefabUnit[] PrefabEntitieResources;


        public MotionPrefabCreator MotionPrefabCreator { get; private set; }


        void Awake()
        {
            var em = World.Active.EntityManager;

            this.MotionPrefabCreator = new MotionPrefabCreator( em );


            var qEntities =
                from prefab in this.Prefabs
                select new PrefabUnit
                {
                    //Prefab = null,
                    Motion = prefab.Convert( this ),
                }
                ;
            this.PrefabEntitieResources = qEntities.ToArray();
        }

        void OnDestroy()
        {
            foreach( var unit in this.PrefabEntitieResources )
                unit.Dispose();
        }


        public abstract class IConvertToPrefab : MonoBehaviour
        {
            abstract public IPrefabResourceUnit Convert( PrefabSettingsAuthoring arthur );
        }
        public interface IPrefabResourceUnit : IDisposable
        {}
    }


    public class PrefabUnit : PrefabSettingsAuthoring.IPrefabResourceUnit
    {
        public Entity Prefab;

        public PrefabSettingsAuthoring.IPrefabResourceUnit Motion;


        public void Dispose()
        {
            this.Motion.Dispose();
        }
    }

}

