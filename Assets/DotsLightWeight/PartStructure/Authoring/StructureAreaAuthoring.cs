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
using UnityEditor;
using Unity.Physics;

using Collider = Unity.Physics.Collider;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using Unity.Physics.Authoring;
    using System.Runtime.InteropServices;
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Structure.Authoring;
    using DotsLite.EntityTrimmer.Authoring;


    //binder
    //main
    // posture
    // draw instance
    // collider
    // destructions
    //part
    // debris prefab
    //bone
    // bone id

    public class StructureAreaAuthoring : ModelGroupAuthoring.ModelAuthoringBase//, IConvertGameObjectToEntity
    {

        public AreaModel<UI32, StructureVertex> NearModel;

        public GameObject Envelope;
        public StructureAreaAuthoring MasterPrefab;

        [SerializeField]
        ColorPalletAsset Pallet;


        public override IEnumerable<IMeshModel> QueryModel =>
            new IMeshModel[] { this.NearModel };

        [SerializeField]
        public SourcePrefabKeyUnit key;



        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            CreateStructureEntities_Compound(conversionSystem);

        }

        void CreateStructureEntities_Compound(GameObjectConversionSystem gcs)
        {
            var top = this;
            var near = this.NearModel.Obj;
            var env = this.Envelope;
            var posture = this.GetComponentInChildren<PostureAuthoring>();
            var parts = near.GetComponentsInChildren<StructureAreaPartAuthoring>();
            //var bones = near.GetComponentsInChildren<StructureBone>();

            parts.DispatchPartId();

            this.WrapEnumerable<ModelGroupAuthoring.ModelAuthoringBase>()
                .Concat(parts)
                .BuildModelToDictionary(gcs);

            initBinderEntity_(gcs, top, posture);
            gcs.InitPostureEntity(posture);//, far.objectTop.transform);
            initMainEntity_(gcs, top, posture, this.NearModel, parts.Length);

            //// とりあえず１ボーンに対応
            //var bone0 = bones.First();
            //var qParts0 = parts
            //    .Where(x => x.GetComponentInParent<StructureBone>().BoneId == 0);
            //initStructureBone(gcs, posture, bone0, qParts0);

            //setBoneForFarEntity_(gcs, posture, far, top.transform);
            //setBoneForNearSingleEntity_(gcs, posture, near, near.transform);

            trimEntities_(gcs, this);
            orderTrimEntities_(gcs, this);
        }

        static void initBinderEntity_
            (GameObjectConversionSystem gcs, StructureAreaAuthoring top, PostureAuthoring main)
        {
            var em_ = gcs.DstEntityManager;

            var binderEntity = gcs.GetPrimaryEntity(top);
            var mainEntity = gcs.GetPrimaryEntity(main);


            var binderAddtypes = new ComponentTypes
            (
                typeof(BinderTrimBlankLinkedEntityGroupTag),
                typeof(ObjectBinder.MainEntityLinkData)
            );
            em_.AddComponents(binderEntity, binderAddtypes);

            em_.SetComponentData(binderEntity,
                new ObjectBinder.MainEntityLinkData { MainEntity = mainEntity });


            em_.SetName_(binderEntity, $"{top.name} binder");
        }

        static void initMainEntity_(
            GameObjectConversionSystem gcs, StructureAreaAuthoring top, PostureAuthoring main,
            IMeshModelLod near, int partLength)
        {
            var em = gcs.DstEntityManager;

            var binderEntity = gcs.GetPrimaryEntity(top);
            var mainEntity = gcs.GetPrimaryEntity(main);


            var mainAddtypes = new ComponentTypes(new ComponentType[]
            {
                typeof(Main.CompoundColliderTag),
                typeof(Main.MainTag),
                typeof(Main.BinderLinkData),//暫定
                typeof(Main.PartDestructionData),
                typeof(Main.PartLengthData),
                //typeof(Main.SleepTimerData),

                typeof(DrawInstance.MeshTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),
                typeof(DrawInstance.NeedLodCurrentTag),
                typeof(DrawInstance.LodCurrentIsNearTag),

                typeof(Collision.Hit.TargetData),
                //typeof(NonUniformScale),//暫定
                //typeof(ObjectMain.ObjectMainTag),
                //typeof(PhysicsCollider),
                typeof(Marker.Rotation),
                typeof(Marker.Translation),
            });
            em.AddComponents(mainEntity, mainAddtypes);

            em.CopyTransformToMarker(mainEntity, main.transform);


            //em.SetComponentData(mainEntity, new NonUniformScale
            //{
            //    Value = new float3(1, 1, 1)
            //});
            em.SetComponentData(mainEntity, new Main.BinderLinkData
            {
                BinderEntity = binderEntity,
            });
            em.SetComponentData(mainEntity, new DrawInstance.ModelLinkData
            {
                DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(near.SourcePrefabKey),
                //DrawModelEntityCurrent = Entity.Null,//gcs_.GetFromModelEntityDictionary(far_.objectTop),//(top_),
                //DrawModelEntityCurrent = mainEntity,// ダミーとして、モデルでないものを入れとく（危険かなぁ…）
                // 最初のＬＯＤ判定で Null もタグ付けさせるため
            });
            em.SetComponentData(mainEntity, new DrawInstance.TargetWorkData
            {
                DrawInstanceId = -1,
            });
            em.SetComponentData(mainEntity,
                new Collision.Hit.TargetData
                {
                    MainEntity = gcs.GetPrimaryEntity(main),
                    HitType = Collision.HitType.envelope,
                }
            );

            em.SetComponentData(mainEntity, new Main.PartLengthData
            {
                TotalPartLength = partLength,
                BoneLength = 1,
            });
            //em.SetComponentData(mainEntity,
            //    new Main.SleepTimerData
            //    {
            //        PrePositionAndTime = new float4(0, 0, 0, Main.SleepTimerData.Margin),
            //    }
            //);

            var draw = mainEntity;
            gcs.AddLod1ComponentToDrawInstanceEntity(draw, top.gameObject, near);


            em.SetName_(mainEntity, $"{top.name} main");
        }

        //static void initCompoundColliderEntity(GameObjectConversionSystem gcs, GameObject main, StructureAreaPartAuthoring[] parts)
        //{
        //    var em = gcs.DstEntityManager;
        //    Debug.Log(main.name);

        //    var ent = gcs.GetPrimaryEntity(main);
        //    em.AddComponentData(ent, new LateBuildCompoundColliderConversion.TargetData
        //    {
        //        Dst = main,
        //        Srcs = parts.Select(x => x.gameObject),
        //    });
        //}

        //public void initBoneColliders(
        //    GameObjectConversionSystem gcs, StructureBone bone,
        //    IEnumerable<StructureAreaPartAuthoring> parts, CollisionFilter filter)
        //{
        //    var em = gcs.DstEntityManager;
        //    var mtinv = bone.transform.worldToLocalMatrix;

        //    var ent = gcs.GetPrimaryEntity(bone);
        //    var buffer = em.AddBuffer<>

        //    foreach (var p in parts)
        //    {
        //        var mt = p.transform.localToWorldMatrix * mtinv;


        //    }
        //}
        //public BlobAssetReference<Collider> createCompoundCollider(StructureAreaPartAuthoring[] parts, CollisionFilter filter)
        //{
        //    var dst = new NativeArray<CompoundCollider.ColliderBlobInstance>(
        //        parts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        //    for (var i = 0; i < parts.Length; i++)
        //    {
        //        dst[i] = new CompoundCollider.ColliderBlobInstance
        //        {
        //            Collider = BlobAssetReference<Collider>.Null,
        //            CompoundFromChild = new RigidTransform
        //            {
        //                pos = float3.zero,
        //                rot = quaternion.identity,
        //            }
        //        };
        //    }

        //    var res = CompoundCollider.Create(dst);
        //    dst.Dispose();
        //    return res;
        //}


        /// <summary>
        /// main, near, far, part 以外の entity を削除する
        /// </summary>
        static void trimEntities_(GameObjectConversionSystem gcs, StructureAreaAuthoring st)
        {
            var em = gcs.DstEntityManager;

            //var top = st.gameObject;
            //var far = st.FarModel.Obj;
            var near = st.NearModel.Obj;
            var env = st.Envelope;
            var main = env;

            var needs = st.GetComponentsInChildren<StructureAreaPartAuthoring>()
                .Select(x => x.gameObject)
                //.Append(top)
                .Append(main)
                .Append(near)
                //.Append(far)
                ;
            foreach (var obj in st.gameObject.Descendants().Except(needs))
            {
                var ent = gcs.GetPrimaryEntity(obj);
                em.DestroyEntity(ent);
            }
        }
        /// <summary>
        /// part entity の遅延的破棄をセットする
        /// </summary>
        static void orderTrimEntities_(GameObjectConversionSystem gcs, StructureAreaAuthoring st)
        {
            var em = gcs.DstEntityManager;

            var qEnt = st.GetComponentsInChildren<StructureAreaPartAuthoring>()
                .Select(x => gcs.GetPrimaryEntity(x));
            em.AddComponentData(qEnt, new DestroyEntityLateConversion.TargetTag { });
        }

    }
}
