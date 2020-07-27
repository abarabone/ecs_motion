﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;

namespace Abarabone.Structure.Authoring
{

    using Abarabone.Model;
    using Abarabone.Draw.Authoring;
	using Abarabone.Geometry;
	using Abarabone.Common.Extension;
    using Unity.Physics.Authoring;

    public class StructurePartAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {


        [HideInInspector]
        public int PartId;
        //public int Life;

        public Material Material;

        public GameObject MasterPrefab;



        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    Debug.Log(this.name+" prefab");
        //    //var go = Instantiate(this.gameObject);
        //    //referencedPrefabs.Add(go);
        //}


        /// <summary>
        /// 
        /// </summary>
        public async void Convert
            (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            Debug.Log("pt auth "+this.name);

            //var go = Instantiate(this.gameObject);
            //go.AddComponent<PhysicsBodyAuthoring>();
            //Debug.Log(conversionSystem.GetPrimaryEntity(go));

            //initPartData_(conversionSystem, this.gameObject, this.PartId);

            return;



            void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject part, Material srcMaterial, Mesh mesh_)
            {
                var mat = new Material(srcMaterial);

                const BoneType BoneType = BoneType.TR;
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents(part, mesh_, mat, BoneType, boneLength);
            }

            //void initPartData_
            //    (GameObjectConversionSystem gcs, GameObject part, int partId)
            //{
            //    var em = gcs.DstEntityManager;

            //    var ent = gcs.GetPrimaryEntity(part);

            //    em.AddComponentData(ent,
            //        new StructurePart.PartData
            //        {
            //            //PartId = partId,
            //            //Life = 
            //        }
            //    );
            //}

        }


    }
}
