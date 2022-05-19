using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;
using System;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Geometry;

    [Serializable]
    public class PositionNormalUvI32Part :
        PartModel<UI32, PositionNormalUvVertex>, StructureBuildingPartAuthoring.IMeshModelSelector
    { }
}

namespace DotsLite.Structure.Authoring
{

    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
	using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.Character;//ObjectMain はここにある、名前変えるべきか？
    using DotsLite.Structure;
    using Unity.Physics;
    using Unity.Transforms;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.EntityTrimmer.Authoring;

    using Material = UnityEngine.Material;
    using Unity.Physics.Authoring;

    public class StructureBuildingPartAuthoring : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity, IStructurePart
    {


        public int PartId;
        public int partId { get => this.PartId; set => this.PartId = value; }
        //public int Life;

        //public Material MaterialToDraw;

        //public GameObject MasterPrefab;

        public interface IMeshModelSelector : IMeshModel
        { }
        [SerializeReference, SubclassSelector]
        public IMeshModelSelector PartModel;
        //public PartModel<UI32, PositionNormalUvVertex> PartModel;

        public ColorPaletteAsset Palette;


        public override IEnumerable<IMeshModel> QueryModel =>
            //new PartModel<UI32, PositionNormalUvVertex>(this.MasterPrefab, this.MaterialToDraw.shader)
            this.PartModel
            .WrapEnumerable();



        /// <summary>
        /// パーツインスタンスエンティティを生成する。モデルは同じパーツで１つ。
        /// 破壊されたときの落下プレハブもインスタンス単位で作成しているが、破壊時に位置を仕込んでいるため、１つでよいのでは？
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            //Debug.Log("pt auth "+this.name);

            //var topAuth = this.gameObject
            //    //.Ancestors()
            //    //.Select(go => go.GetComponent<StructureBuildingModelAuthoring>())
            //    //.First(x => x != null);
            //    .GetComponentsInParent<StructureBuildingModelAuthoring>(true)
            //    .FirstOrDefault();
            //var top = topAuth.gameObject;
            //var main = topAuth.FarMeshObject.objectTop;
            //var main = topAuth.FarModel.Obj;

            //return;
            //if (!dstManager.Exists(entity)) return;//


            this.QueryModel.BuildModelToDictionary(conversionSystem);
            var modelEntity = conversionSystem.GetFromModelEntityDictionary(this.PartModel.SourcePrefabKey);


            var posture = this.FindParent<StructureBuildingAuthoring>()
                .GetComponentInChildren<PostureAuthoring>();

            initPartData_(conversionSystem, posture, this, this.PartId);

            //createMeshAndSetToDictionary_(conversionSystem, this.MasterPrefab, this.GetPartsMeshesAndFuncs);
            //createModelEntity_IfNotExists_(conversionSystem, this.MasterPrefab, this.MaterialToDraw);
            var prefab = StructurePartUtility.CreateDebrisPrefab(conversionSystem, this.gameObject, modelEntity);
            var part = conversionSystem.GetPrimaryEntity(this.gameObject);

            setPrefabToPart_(conversionSystem, part, prefab);

            return;



            static void initPartData_(
                GameObjectConversionSystem gcs, PostureAuthoring main, StructureBuildingPartAuthoring part, int partId)
            {
                var em = gcs.DstEntityManager;

                var ent = gcs.GetPrimaryEntity(part);
                var addtypes = new ComponentTypes
                (
                    typeof(Part.PartData),
                    typeof(Collision.Hit.TargetData),
                    typeof(Disabled),

                    typeof(Marker.Translation),
                    typeof(Marker.Rotation)
                );
                em.AddComponents(ent, addtypes);

                em.CopyTransformToMarker(ent, part.transform);



                em.SetComponentData(ent,
                    new Part.PartData
                    {
                        PartId = partId,
                        //Life = 
                    }
                );
                em.SetComponentData(ent,
                    new Collision.Hit.TargetData
                    {
                        MainEntity = gcs.GetPrimaryEntity(main),
                        HitType = Collision.HitType.part,
                    }
                );
            }

            void setPrefabToPart_(GameObjectConversionSystem gcs, Entity part, Entity prefab)
            {
                var em = gcs.DstEntityManager;

                em.AddComponentData(part,
                    new Part.DebrisPrefabData
                    {
                        DebrisPrefab = prefab,
                    }
                );
            }

            //static void createMeshAndSetToDictionary_
            //    (GameObjectConversionSystem gcs, GameObject go, Func<(GameObject go, Func<MeshCombinerElements> f, Mesh mesh)> meshHolder)
            //{
            //    if (gcs.IsExistingInMeshDictionary(go)) return;

            //    var x = meshHolder();
            //    var newmesh = x.mesh ?? x.f().CreateMesh();

            //    Debug.Log($"part model {go.name} - {newmesh.name}");

            //    gcs.AddToMeshDictionary(go, newmesh);
            //}


            //static void createModelEntity_IfNotExists_
            //    (GameObjectConversionSystem gcs, GameObject masterPrefab, Material shader)
            //{
            //    if (gcs.IsExistsInModelEntityDictionary(masterPrefab)) return;

            //    var mesh = gcs.GetFromMeshDictionary(masterPrefab);
            //    var mat = new Material(shader);

            //    const BoneType BoneType = BoneType.TR;// あとでＳもつける
            //    const int boneLength = 1;

            //    var modelEntity_ = gcs.CreateDrawModelEntityComponents(masterPrefab, mesh, mat, BoneType, boneLength);
            //}




            ///// <summary>
            ///// 元となったプレハブから自身と子を合成する関数を取得する。ただし子からパーツは除外する。
            ///// また、オブジェクトが１つの時は直接メッシュを取得する。
            ///// </summary>
            //public (GameObject go, Func<MeshCombinerElements> f, Mesh mesh) GetPartsMeshesAndFuncs()
            //{

            //    var part = this.MasterPrefab;
            //    var children = queryPartBodyObjects_Recursive_(part);//.ToArray();

            //    var isSingle = children.IsSingle();//children.Length == 1;
            //    var f = !isSingle
            //        ? MeshCombiner.BuildNormalMeshElements(children, part.transform)
            //        : null;
            //    var mesh = isSingle
            //        ? children.First().GetComponent<MeshFilter>().sharedMesh
            //        : null;

            //    return (part, f, mesh);


            //    static IEnumerable<GameObject> queryPartBodyObjects_Recursive_(GameObject go)
            //    {
            //        var q =
            //            from child in go.Children()
            //            where child.GetComponent<StructurePartAuthoring>() == null
            //            from x in queryPartBodyObjects_Recursive_(child)
            //            select x
            //            ;
            //        return q.Prepend(go);
            //    }
            //}
        }
    }
}
