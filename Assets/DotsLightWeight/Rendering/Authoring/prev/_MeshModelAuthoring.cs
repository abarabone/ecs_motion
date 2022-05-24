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

namespace DotsLite.Model.Authoring.prev
{
    using DotsLite.Geometry;

    //[Serializable]
    //public class PositionNormalUvI32 :
    //    MeshModel<UI32, PositionNormalUvVertex>, MeshModelAuthoring1.IMeshModelSelector
    //{ }

    //[Serializable]
    //public class PositionNormalUvWithPaletteI32 :
    //    MeshWithPaletteModel<UI32, PositionNormalUvWithPaletteVertex>, MeshModelAuthoring1.IMeshModelSelector
    //{ }
}

namespace DotsLite.Model.Authoring.prev
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

    // レンダリング単位＝インスタンシング単位      メッシュとマテリアル
    // ゲームオブジェクト単位                      インスタンス


    public class _MeshModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {


        [SerializeField]
        ColorPaletteAsset Palette;



        /// <summary>
        /// 
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            this.QueryModel.BuildModelToDictionary(conversionSystem);

            var drawInstatnce = initInstanceEntityComponents_(conversionSystem, this.gameObject);

            //conversionSystem.AddLod2ComponentToDrawInstanceEntity(drawInstatnce, this.gameObject, this.Models);

            conversionSystem.SetColorPaletteComponent(this.gameObject, this.Palette);

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
                        DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(this.QueryModel.First().SourcePrefabKey),
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