using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;

namespace Abss.Motion
{
    
    public class CharacterGroupArthuring : MonoBehaviour
    {
        
        public CharactorResourceUnit[] Resources;


        void Awake()
        {
            passResourcesToDrawSystem();
        }

        void passResourcesToDrawSystem()
        {

            foreach( var x in this.Resources.Select((res,id)=>(id,res)) )
            {
                var md = new MotionDataInNative();
                md.ConvertFrom( x.res.MotionClip );
                
            }
        }

        void createEntities( EntityManager em )
        {
            foreach( var x in this.Resources.Select((res,id)=>(id,res)) )
            {
                addMotionComponentData( x.id,  );
            }
        }

        void addMotionComponentData( EntityManager em, int renderingId )
        {
            var ent = em.CreateEntity( )
            dstManager.AddComponentData( ent, new MotionInfoData() );

        }
        
        void addStreamComponentData
            (
                int renderingId, MotionDataAccessor ma,
                EntityManager dstManager, GameObjectConversionSystem conversionSystem
            )
        {
            //conversionSystem.
        }
    }

    [Serializable]
    public struct CharactorResourceUnit
    {
        public Mesh[] SkinnedMesh;
        public Material Material;
        public MotionClip MotionClip;
    }


    public class MeshRenderingHolder
    {

        public List<MeshRenderingUnit> Units { get; } = new List<MeshRenderingUnit>();


        public int Add( Mesh mesh, Material material )
        {
            var meshId = this.Units.Count + 1;

            this.Units.Add(
                new MeshRenderingUnit
                {
                    MeshId = meshId,
                    Mesh = mesh,
                    Material = material,
                }
            );

            return meshId;
        }
    }

    public class MeshRenderingUnit
    {
        public int MeshId;
        public Mesh Mesh;
        public Material Material;
    }
}


