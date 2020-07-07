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
            return;

            var meshElements = await combinePartMeshesAsync_(this);

            var mesh = meshElements.CreateMesh();

            createModelEntity_(conversionSystem, this.gameObject, this.Material, mesh);

            return;



            void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject main, Material srcMaterial, Mesh mesh_)
            {
                var mat = new Material(srcMaterial);

                const BoneType BoneType = BoneType.TR;
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents(main, mesh_, mat, BoneType, boneLength);
            }


            /// <summary>
            /// パーツが子以下の改装にメッシュを持っていた場合、１つのメッシュとなるように結合する。
            /// </summary>
            async Task<MeshElements> combinePartMeshesAsync_( StructurePartAuthoring part_ )
            {

                // 子孫にメッシュが存在すれば、引っ張ってきて結合。１つのメッシュにする。
                // （ただしパーツだった場合は、結合対象から除外する）
                var buildTargets = queryTargets_Recursive_(part_.gameObject).ToArray();
                if (buildTargets.Length == 1) return new MeshElements { };

                var meshElements_ = await combineChildMeshesAsync_(buildTargets, part_.transform);

                return meshElements_;


                IEnumerable<GameObject> queryTargets_Recursive_(GameObject go_)
                {
                    var q =
                        from child in go_.Children()
                        where child.GetComponent<StructurePartAuthoring>() == null
                        from x in queryTargets_Recursive_(child)
                        select x
                        ;
                    return q.Prepend(go_);
                }

                async Task<MeshElements> combineChildMeshesAsync_(IEnumerable<GameObject> targets_, Transform tf_)
                {
                    var combineElementFunc =
                        MeshCombiner.BuildNormalMeshElements(targets_, tf_, isCombineSubMeshes: true);

                    return await Task.Run(combineElementFunc);
                }
            }
        }


    }
}
