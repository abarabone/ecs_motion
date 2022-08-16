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

namespace DotsLite.Model.Authoring.Vertex.MeshWithPaletteModel
{
    using DotsLite.Geometry;

    public interface IVertexUnitWithPalette
    { }


    [Serializable]
    public class PositionOnly : PositionVertexBuilder, Authoring.MeshWithPaletteModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUv : PositionUvVertexBuilder, Authoring.MeshWithPaletteModel.IVertexSelector
    { }

    [Serializable]
    public class PositionUvNormal : PositionUvNormalVertexBuilder, Authoring.MeshWithPaletteModel.IVertexSelector
    { }


    //[Serializable]
    //public class PositionOnlyWithPallet : PositionWithPalletVertexBuilder, Authoring.MeshModel.IVertexSelector
    //{ }

    //[Serializable]
    //public class PositionUvWithPallet : PositionUvWithPalletVertexBuilder, Authoring.MeshModel.IVertexSelector
    //{ }

    [Serializable]
    public class PositionUvNormalWithPalette :
        PositionUvNormalWithPaletteVertexBuilder,
        Authoring.MeshWithPaletteModel.IVertexSelector,
        IVertexUnitWithPalette
    { }
}

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Geometry.Palette;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;

    [Serializable]
    public class MeshWithPaletteModel : MeshModelBase
    {


        //[SerializeField]
        //ColorPaletteAsset Palette;


        public override Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var p = new AdditionalParameters();
            var atlas = atlasDictionary.modelToAtlas[this.sourcePrefabKey].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var mmts = this.QueryMmts.ToArray();
            p.calculateParameters(mmts, this.Obj.transform, subtexhash => texdict[atlas, subtexhash]);

            // パレット向けの暫定
            if (this.vtxBuilder is Vertex.MeshWithPaletteModel.IVertexUnitWithPalette)
            {
                p.CalculatePaletteSubIndexParameter(mmts);
            }

            var md = MeshCreatorUtility.AllocateMeshData();
            return () => meshpack.CreateMeshData(md, this.IdxBuilder, this.VtxBuilder, p);
        }


        public override Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var ent = base.CreateModelEntity(gcs, mesh, atlas);

            addLinkData_IfHas_();

            return ent;


            void addLinkData_IfHas_()
            {
                if (this.vtxBuilder is Vertex.MeshWithPaletteModel.IVertexUnitWithPalette == false)
                {
                    return;
                }

                var em = gcs.DstEntityManager;
                em.AddComponentData(ent, new DrawModelShaderBuffer.ColorPaletteLinkData
                {
                    BufferEntity = Entity.Null,//gcs.GetPrimaryEntity(paletteAuthor),
                });
            }
        }



        public interface IVertexSelector : IVertexBuilder { }


        [SerializeReference, SubclassSelector]
        public IIndexSelector idxBuilder;
        protected override IIndexBuilder IdxBuilder => this.idxBuilder;

        [SerializeReference, SubclassSelector]
        public IVertexSelector vtxBuilder;
        protected override IVertexBuilder VtxBuilder => this.vtxBuilder;
    }


    [Serializable]
    public class LodMeshWithPaletteModel : MeshWithPaletteModel, IMeshModelLod
    {

        [SerializeField]
        public float limitDistance;

        [SerializeField]
        public float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }



    public static class ColorPaletteMeshUtility
    {

        /// <summary>
        /// メッシュ構築用のパラメータとして、
        /// パレットのサブインデックスを、サブメッシュ単位で列挙する。
        /// サブインデックスは、マテリアルの palette sub index から取得する。
        /// マテリアルが null の場合は、0 を返す。
        /// </summary>
        public static void CalculatePaletteSubIndexParameter(
            this AdditionalParameters parameters,
            (Mesh mesh, Material[] mats, Transform tf)[] mmts)
        {
            var q =
                from mmt in mmts
                select
                    from mat in mmt.mats
                    select getPaletteSubIndex(mat)
                ;
            parameters.paletteSubIndexPerSubMesh = q.ToArrayRecursive2();


            /// <summary>
            /// マテリアルから、パレットインデックス情報を取得する。
            /// 該当するプロパティがない場合のインデックスは、0 とする。
            /// </summary>
            static int getPaletteSubIndex(Material mat) =>
                //mat?.HasInt("Palette Sub Index") ?? false
                mat?.HasProperty("Palette Sub Index") ?? false
                    ? mat.GetInt("Palette Sub Index")
                    : 0
                ;
        }
    }

}