using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;

namespace Abarabone.Structure.Aurthoring
{

    using Abarabone.Model;
    using Abarabone.Draw.Authoring;
	using Abarabone.Geometry;
	using Abarabone.Common.Extension;
    



    public class StructurePartAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {


        public int PartId;

        public Material Material;




        /// <summary>
        /// 
        /// </summary>
        public async void Convert
            (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            Debug.Log(this.name);

            //var meshElements = combinePartMeshesAsync_(this).Result;
            //var meshElements = combinePartMeshes_(this);

            //var mesh = meshElements.CreateMesh();

            //createModelEntity_(conversionSystem, this.gameObject, this.Material, mesh);

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
