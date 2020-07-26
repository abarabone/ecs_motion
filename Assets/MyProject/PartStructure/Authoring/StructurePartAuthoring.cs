using System.Collections;
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


        //public int PartId;

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


            return;



            void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject main, Material srcMaterial, Mesh mesh_)
            {
                var mat = new Material(srcMaterial);

                const BoneType BoneType = BoneType.TR;
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents(main, mesh_, mat, BoneType, boneLength);
            }


        }


    }
}
