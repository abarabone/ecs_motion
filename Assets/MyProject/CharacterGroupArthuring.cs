using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;

namespace Abss.Motion
{
    
    public class CharacterGroupArthuring : MonoBehaviour, IConvertGameObjectToEntity
    {
        
        public CharactorResourceUnit[] Resources;


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            foreach( var x in this.Resources.Select((res,id)=>(id,res)) )
            {
                var md = new MotionDataInNative();
                md.ConvertFrom( x.res.MotionClip );

                makeChEntities( x.id, md, entity, dstManager, conversionSystem );
            }
        }

        void addMotionComponentData
            (
                int renderingId, MotionDataAccessor ma,
                Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem
            )
        {
            

        }
        
        void addStreamComponentData
            (
                int renderingId, MotionDataAccessor ma,
                EntityManager dstManager, GameObjectConversionSystem conversionSystem
            )
        {
            conversionSystem.
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

            var unit = new MeshRenderingUnit
            {
                MeshId = meshId,
                Mesh = mesh,
                Material = material,
            };
            this.Units.Add( unit );

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


