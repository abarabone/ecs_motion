using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model.Authoring;
    using DotsLite.Geometry;

    public class StructureBuildingAliasAuthoring : ModelGroupAuthoring.ModelAuthoringBase
    {

        public StructureBuildingAuthoring StructureModelPrefab;


        public override IEnumerable<IMeshModel> QueryModel => this.StructureModelPrefab.QueryModel;

    }
}