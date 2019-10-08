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

    public class PrefabSettingsAuthoring : MonoBehaviour
    {

        public ConvertToMainCustomPrefabEntityBehaviour[] PrefabGameObjects;

        public Entity[] PrefabEntities { get; private set; }


        public class PrefabCreators
        {
            public CharactorPrefabCreator   Character;
            public MotionPrefabCreator      Motion;
            public DrawMeshPrefabCreator    Draw;
        }


        void Awake()
        {
            var em = World.Active.EntityManager;

            var prefabCreators = new PrefabCreators
            {
                Character = new CharactorPrefabCreator( em ),
                Motion = new MotionPrefabCreator( em ),
                Draw = new DrawMeshPrefabCreator( em ),
            };

            var drawMeshCsSystem = em.World.GetExistingSystem<DrawMeshCsSystem>();
            var drawMeshCsResourceHolder = drawMeshCsSystem.GetResourceHolder();

            this.PrefabEntities = this.PrefabGameObjects
                .Select( prefab => prefab.Convert( em, drawMeshCsResourceHolder, prefabCreators ) )
                .ToArray();
        }

        void OnDestroy()
        {

        }


        public abstract class ConvertToMainCustomPrefabEntityBehaviour : ConvertToCustomPrefabEntityBehaviour
        { }

        public abstract class ConvertToCustomPrefabEntityBehaviour : MonoBehaviour
        {
            abstract public Entity Convert( EntityManager em, DrawMeshResourceHolder drawres, PrefabCreators creators );
        }
    }


}

