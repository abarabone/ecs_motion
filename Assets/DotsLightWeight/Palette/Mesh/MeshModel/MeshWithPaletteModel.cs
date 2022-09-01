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
    using DotsLite.Geometry.Palette;

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
    public class PositionUvNormalWithColorPalette :
        PositionUvNormalWithColorPaletteVertexBuilder,
        Authoring.MeshWithPaletteModel.IVertexSelector,
        IVertexUnitWithPalette
    { }

    [Serializable]
    public class PositionUvNormalWithColorUvPalette :
        PositionUvNormalWithUvColorPaletteVertexBuilder,
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
    using DotsLite.Geometry.inner.palette;

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
            var atlas = atlasDictionary.modelToAtlas[this].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var mmts = this.QueryMmts.ToArray();
            p.calculateParameters(mmts, this.Obj.transform, subtexhash => texdict[atlas, subtexhash]);

            // パレット向けの暫定
            if (this.vtxBuilder is Vertex.MeshWithPaletteModel.IVertexUnitWithPalette)
            {
                p.CalculatePaletteSubIndexParameter(mmts);
                p.CalculateUvPaletteSubIndexParameter(mmts);
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
                //if (this.vtxBuilder is Vertex.MeshWithPaletteModel.IVertexUnitWithPalette == false)
                //{
                //    return;
                //}

                //var em = gcs.DstEntityManager;
                //em.AddComponentData(ent, new DrawModelWithPalette.ColorPaletteLinkData
                //{
                //    ShaderBufferEntity = Entity.Null,//gcs.GetPrimaryEntity(paletteAuthor),
                //});

                var em = gcs.DstEntityManager;

                switch (this.vtxBuilder)
                {
                    case Vertex.MeshWithPaletteModel.PositionUvNormalWithColorPalette _:
                        {

                            em.AddComponentData(ent, new DrawModelWithPalette.ColorPaletteLinkData
                            {
                                ShaderBufferEntity = Entity.Null,//gcs.GetPrimaryEntity(paletteAuthor),
                            });

                        }
                        break;

                    case Vertex.MeshWithPaletteModel.PositionUvNormalWithColorUvPalette _:
                        {

                            em.AddComponentData(ent, new DrawModelWithPalette.ColorPaletteLinkData
                            {
                                ShaderBufferEntity = Entity.Null,//gcs.GetPrimaryEntity(paletteAuthor),
                            });

                            em.AddComponentData(ent, new DrawModelWithPalette.UvPaletteLinkData
                            {
                                ShaderBufferEntity = gcs.GetUvPaletteEntity(atlas),
                            });

                        }
                        break;
                }
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


}