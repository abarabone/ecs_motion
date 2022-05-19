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

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Geometry;

    [Serializable]
    public class PositionNormalUvI32AreaPart :
        AreaPartModel<UI32, PositionNormalUvVertex>, StructureAreaPartAuthoring.IMeshModelSelector
    { }
}

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

    public class StructureAreaPartAuthoring : ModelGroupAuthoring.ModelAuthoringBase, IStructurePart//, IConvertGameObjectToEntity
    {
        public bool DoNotPathDeform;            // メッシュをパス変形させない
        //public bool IsPathProjectionToTerrain;  // パスメッシュを地形にそって変形させる
        public bool NoDebris;
        public bool UseUpInterpolation;
        public bool IsStretchBoxMesh;

        public int PartId;
        public int partId { get => this.PartId; set => this.PartId = value; }

        public interface IMeshModelSelector : IMeshModel
        { }
        [SerializeReference, SubclassSelector]
        public IMeshModelSelector PartModel;
        //public AreaPartModel<UI32, PositionNormalUvVertex> PartModel;

        public ColorPaletteAsset Palette;



        public override IEnumerable<IMeshModel> QueryModel =>
            //(this.IsStretchBoxMesh
            //    ? (IMeshModel)new StretchBoxModel<UI32, PositionNormalVertex>
            //    {
            //        objectTop = this.PartModel.objectTop,
            //        shader = this.PartModel.shader
            //    }
            //    : this.PartModel)
            this.PartModel
            .WrapEnumerable();

        //public void Convert
        //    (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        //{
        //    dstManager.
        //}
    }
}