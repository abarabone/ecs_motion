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

    using Abarabone.Common.Extension;

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
            //createDrawInstanceEntity_(conversionSystem, top);

            initBinderEntity_(conversionSystem, top, main);
            initMainEntity_(conversionSystem, top, main);

            return;



            void createModelEntity_
                (
                    GameObjectConversionSystem gcs_, GameObject top_,
                    Material srcMaterial
                )
            {
                var mat = new Material(srcMaterial);
                var mesh = gcs_.GetFromStructureMeshDictionary(top_);

                const BoneType boneType = BoneType.TR;
                const int boneLength = 1;

                gcs_.CreateDrawModelEntityComponents(top_, mesh, mat, boneType, boneLength);
            }



            Entity createDrawInstanceEntity_
                (GameObjectConversionSystem gcs, GameObject top_)
            {
                var em = gcs.DstEntityManager;


                var archetype = em.CreateArchetype
                (
                    typeof(DrawInstance.ModeLinkData),
                    typeof(DrawInstance.TargetWorkData)
                );
                var ent = gcs.CreateAdditionalEntity(top_, archetype);

                em.SetComponentData(ent,
                    new DrawInstance.ModeLinkData
                    {
                        DrawModelEntity = gcs.GetFromModelEntityDictionary(top_),
                    }
                );
                em.SetComponentData(ent,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );


                em.SetName(ent, $"{top_.name} draw");
                return ent;
            }


            void initBinderEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(main_);


                var binderAddtypes = new ComponentTypes
                (
                    typeof(BinderTrimBlankLinkedEntityGroupTag),
                    typeof(ObjectBinder.MainEntityLinkData)
                );
                em_.AddComponents(binderEntity, binderAddtypes);

                em_.SetComponentData(binderEntity,
                    new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


                em_.SetName(binderEntity, $"{top_.name} binder");
            }

            void initMainEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
            {
                var em_ = gcs_.DstEntityManager;

                var binderEntity = gcs_.GetPrimaryEntity(top_);
                var mainEntity = gcs_.GetPrimaryEntity(main_);


                var mainAddtypes = new ComponentTypes
                (
                    //typeof(Abarabone.Particle.ParticleTag),//暫定
                    //typeof(NonUniformScale),//暫定
                    typeof(ObjectMain.ObjectMainTag),
                    typeof(ObjectMain.BinderLinkData),
                    //typeof(DrawTransform.LinkData),//
                    //typeof(DrawTransform.IndexData),//
                    //typeof(DrawTransform.TargetWorkData)//
                    typeof(DrawInstance.ModeLinkData),
                    typeof(DrawInstance.TargetWorkData)
                );
                em_.AddComponents(mainEntity, mainAddtypes);

                //em_.SetComponentData(mainEntity,
                //    new NonUniformScale
                //    {
                //        Value = new float3(1, 1, 1)
                //    }
                //);
                em_.SetComponentData(mainEntity,
                    new ObjectMain.BinderLinkData
                    {
                        BinderEntity = binderEntity,
                    }
                );
                em_.SetComponentData(mainEntity,
                    new DrawInstance.ModeLinkData
                    {
                        DrawModelEntity = gcs_.GetFromModelEntityDictionary(top_),
                    }
                );
                em_.SetComponentData(mainEntity,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );


                em_.SetName(mainEntity, $"{top_.name} main");
            }

        }


    }


}
