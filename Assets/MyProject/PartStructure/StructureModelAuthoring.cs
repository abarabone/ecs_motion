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

            //var skinnedMeshRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>();
            //var qMesh = skinnedMeshRenderers.Select(x => x.sharedMesh);
            //var bones = skinnedMeshRenderers.First().bones.Where(x => !x.name.StartsWith("_")).ToArray();

            //var top = this.gameObject;
            //var main = top.transform.GetChild(0).gameObject;

            //createModelEntity_(conversionSystem, top, this.MaterialToDraw, qMesh, bones);

            //initObjectEntity_(conversionSystem, top, main);

            //return;


            //void createModelEntity_
            //    (
            //        GameObjectConversionSystem gcs_, GameObject top_,
            //        Material srcMaterial, IEnumerable<Mesh> srcMeshes, Transform[] bones_
            //    )
            //{
            //    var mat = new Material(srcMaterial);
            //    var mesh = DrawModelEntityConvertUtility.CombineAndConvertMesh(srcMeshes, bones_);

            //    const BoneType BoneType = BoneType.TR;

            //    gcs_.CreateDrawModelEntityComponents(top_, mesh, mat, BoneType, bones_.Length);
            //}

            //void initObjectEntity_(GameObjectConversionSystem gcs_, GameObject top_, GameObject main_)
            //{
            //    var em_ = gcs_.DstEntityManager;

            //    var binderEntity = gcs_.GetPrimaryEntity(top_);
            //    var mainEntity = gcs_.GetPrimaryEntity(main_);


            //    em_.AddComponentData(binderEntity,
            //        new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


            //    var addtypes = new ComponentTypes
            //    (
            //        typeof(ObjectMain.ObjectMainTag),
            //        typeof(ObjectMain.BinderLinkData),
            //        typeof(ObjectMainCharacterLinkData)
            //    //typeof(ObjectMain.MotionLinkDate)
            //    );
            //    em_.AddComponents(mainEntity, addtypes);


            //    em_.SetComponentData(mainEntity,
            //        new ObjectMain.BinderLinkData
            //        {
            //            BinderEntity = binderEntity,
            //        }
            //    );

            //    em_.SetComponentData(mainEntity,
            //        new ObjectMainCharacterLinkData
            //        {
            //            PostureEntity = mainEntity,//
            //        }
            //    );

            //    em_.SetName(binderEntity, $"{top_.name} binder");
            //    em_.SetName(mainEntity, $"{top_.name} main");
            //}

        }


    }


}
