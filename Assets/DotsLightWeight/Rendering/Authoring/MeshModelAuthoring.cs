using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.EntityTrimmer.Authoring;

    /// <summary>
    /// this.Model にセットされた MeshModel を、モデルとしてコンバートする。
    /// Nothing であれば、ツリー内に存在する MeshModel を対象とする。
    /// また model の .SourcePrefabKey が同じモデルは、同じものとみなす。
    /// （当初は同じプレハブからコンバートしたモデルで賄おうとしたんだけど、構造物パーツなんかでインスタンスとモデルを作る手間が面倒なので）
    /// </summary>
    public class MeshModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity//, DeclareReferencedPrefabs
    {

        //public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        //{
        //    if (this.Model == null) return;

        //    referencedPrefabs.Add(this.Model.AsGameObject);
        //}

        public override IEnumerable<IMeshModel> QueryModel => this._model.WrapEnumerable();
        IMeshModel _model => this.Model ?? this.GetComponentInChildren<IMeshModel>();
        //public override IEnumerable<IMeshModel> QueryModel
        //{
        //    get
        //    {
        //        var qMain = this.gameObject.WrapEnumerable()
        //            .Select(x => new MeshModel<UI32, PositionNormalUvVertex>
        //            {
        //                objectTop = x,
        //                shader = this.ShaderToDraw,
        //            })
        //            .Cast<IMeshModel>()
        //            .Where(x => this.Models == null || this.Models?.Length == 0);

        //        var qLod = this.Models
        //            //.Select(x => x.obj = x.obj ?? this.gameObject)
        //            .Cast<IMeshModel>();

        //        return qMain.Concat(qLod)
        //            .Distinct();
        //    }
        //}



        public MeshModel Model;




        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            this.QueryModel.BuildModelToDictionary(conversionSystem);

            var drawInstatnce = initInstanceEntityComponents_(conversionSystem, this.gameObject);

            //conversionSystem.AddLod2ComponentToDrawInstanceEntity(drawInstatnce, this.gameObject, this.Models);

            return;




            Entity initInstanceEntityComponents_(GameObjectConversionSystem gcs, GameObject main)
            {
                dstManager.SetName_(entity, $"{this.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var archetype = em.CreateArchetype(
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(BinderTrimBlankLinkedEntityGroupTag),
                    typeof(DrawInstance.MeshTag),
                    typeof(DrawInstance.ModelLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(Marker.Translation),
                    typeof(Marker.Rotation)
                    //typeof(Marker.NonUniformScale)
                );
                em.SetArchetype(mainEntity, archetype);

                em.CopyTransformToMarker(mainEntity, main.transform);


                em.SetComponentData(mainEntity,
                    new DrawInstance.ModelLinkData
                    //new DrawTransform.LinkData
                    {
                        //DrawModelEntityCurrent = gcs.GetPrimaryEntity(this.QueryModel.First().AsGameObject),
                        DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(this.QueryModel.Single()),
                    }
                );
                em.SetComponentData(mainEntity,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                //em.SetComponentData(mainEntity,
                //    new Marker.Translation
                //    {
                //        Value = float3.zero,
                //    }
                //);
                //em.SetComponentData(mainEntity,
                //    new Marker.Rotation
                //    {
                //        Value = quaternion.identity,
                //    }
                //);
                //em.SetComponentData(mainEntity,
                //    new Marker.NonUniformScale
                //    {
                //        Value = new float3(1.0f, 1.0f, 1.0f),
                //    }
                //);

                return mainEntity;
            }

        }

    }
}