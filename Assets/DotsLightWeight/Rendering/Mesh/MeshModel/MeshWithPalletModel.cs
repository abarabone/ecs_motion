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
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;

    [Serializable]
    public class MeshWithPaletteModel : MeshModel
    {


        [SerializeField]
        ColorPaletteAsset Palette;


        public override Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var atlas = atlasDictionary.modelToAtlas[this.sourcePrefabKey].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var p = this.QueryMmts.calculateParameters(
                this.Obj.transform, this.QueryBones?.ToArray(), subtexhash => texdict[atlas, subtexhash], null);

            // パレット向けの暫定
            this.QueryMmts.CalculatePaletteSubIndexParameter(ref p);

            var md = MeshCreatorUtility.AllocateMeshData();
            return () => meshpack.CreateMeshData(md, this.IdxBuilder, this.VtxBuilder, p);
        }


        public override Entity CreateModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var ent = base.CreateModelEntity(gcs, mesh, atlas);

            var paletteAuthor = this.GetComponentInParent<ColorPaletteBufferAuthoring>();
            if (paletteAuthor == null) return ent;

            var em = gcs.DstEntityManager;
            em.AddComponentData(ent, new DrawModelShaderBuffer.ColorPaletteLinkData
            {
                BufferEntity = gcs.GetPrimaryEntity(paletteAuthor),
            });

            return ent;
        }

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