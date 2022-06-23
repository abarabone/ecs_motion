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

    public interface IVertexUnitWithPallet
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
        IVertexUnitWithPallet
    { }
}

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
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
            if (this.vtxBuilder is Vertex.MeshWithPaletteModel.IVertexUnitWithPallet)
            {
                p.CalculatePaletteSubIndexParameter(mmts);
            }

            var md = MeshCreatorUtility.AllocateMeshData();
            return () => meshpack.CreateMeshData(md, this.IdxBuilder, this.VtxBuilder, p);
        }


        public override Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var ent = base.CreateModelEntity(gcs, mesh, atlas);

            if (this.vtxBuilder is Vertex.MeshWithPaletteModel.IVertexUnitWithPallet == false)
            {
                return ent;
            }

            var paletteAuthor = this.GetComponentInParent<ColorPaletteBufferAuthoring>();
            if (paletteAuthor == null) return ent;

            var em = gcs.DstEntityManager;
            em.AddComponentData(ent, new DrawModelShaderBuffer.ColorPaletteLinkData
            {
                BufferEntity = gcs.GetPrimaryEntity(paletteAuthor),
            });

            return ent;
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
}