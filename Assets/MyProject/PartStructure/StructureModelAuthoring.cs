using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System.Threading.Tasks;
using Unity.Linq;

namespace Abarabone.Structure.Aurthoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Structure.Authoring;
    using Abarabone.Character;//ObjectMain はここにある、名前変えるべきか？


    /// <summary>
    /// 
    /// </summary>
    public class StructureModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {


        public Material MaterialToDraw;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var parts = this.GetComponentsInChildren<StructurePartAuthoring>();

            var qMasterPrefab = parts
                .Select(x => x.gameObject)
                .Select(x => UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(x));

            referencedPrefabs.AddRange( qMasterPrefab );
        }


        /// <summary>
        /// 
        /// </summary>
        public async void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject;
            var main = top.transform.GetChild(0).gameObject;

            createModelEntity_(conversionSystem, top, this.MaterialToDraw);

            initObjectEntity_(conversionSystem, top, main);

            return;


            void createModelEntity_
                (
                    GameObjectConversionSystem gcs_, GameObject top_,
                    Material srcMaterial
                )
            {
                var mat = new Material(srcMaterial);
                var mesh = gcs_.GetFromStructureMeshDictionary(top_);

                const BoneType BoneType = BoneType.TR;
                const int boneLength = 1;

                gcs_.CreateDrawModelEntityComponents(top_, mesh, mat, BoneType, boneLength);
            }

            void initObjectEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(main_);


                em_.AddComponentData(binderEntity,
                    new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


                var addtypes = new ComponentTypes
                (
                    //typeof(Abarabone.Particle.ParticleTag),//暫定
                    typeof(NonUniformScale),//暫定
                    typeof(ObjectMain.ObjectMainTag),
                    typeof(ObjectMain.BinderLinkData)
                );
                em_.AddComponents(mainEntity, addtypes);


                em_.SetComponentData(mainEntity,
                    new ObjectMain.BinderLinkData
                    {
                        BinderEntity = binderEntity,
                    }
                );


                em_.SetName(binderEntity, $"{top_.name} binder");
                em_.SetName(mainEntity, $"{top_.name} main");
            }

        }


    }


}
