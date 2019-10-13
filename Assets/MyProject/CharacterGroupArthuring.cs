
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
    /*
    [Serializable]
    public struct CharactorResourceUnit
    {
        public Mesh[] SkinnedMesh;
        public Material Material;
        public MotionClip MotionClip;
    }


    public class CharacterGroupArthuring : MonoBehaviour
    {
        
        public CharactorResourceUnit[] Resources;


        MotionPrefabHolder      motionPrefabHolder;
        CharactorPrefabHolder   chPrefabHolder;
        

        List<Entity> ents = new List<Entity>();



        void Awake()
        {
            var w = World.Active;
            var em = w.EntityManager;

            passResourcesToDrawSystem( w, this.Resources );

            this.motionPrefabHolder = new MotionPrefabHolder( em, this.Resources );
            //this.chPrefabHolder = new CharactorPrefabHolder( em, this.Resources );
            
            //var dat = this.motionPrefabDatas[0];
            //var ent = em.Instantiate( dat.Prefab );

            //this.ents.Add( ent );
        }


        private void Update()
        {
            if( !Input.GetMouseButtonDown(0) ) return;
            
            foreach( var x in this.ents ) World.Active.EntityManager.DestroyEntity(x);
        }


        private void OnDisable()
        {
            this.motionPrefabHolder.Dispose();
            this.chPrefabHolder.Dispose();
        }


        

        void passResourcesToDrawSystem( World w, CharactorResourceUnit[] resources )
        {
            var drawSystem = w.GetExistingSystem<DrawMeshCsSystem>();

            //drawSystem.resourceHolder.AddDrawMeshResources( resources );
        }
        
    }

    */


}


