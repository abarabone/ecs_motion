using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;



    public static partial class MeshCombineUtility
    {


        public static void calculatePartParameters(
            this AdditionalParameters parameters, (Mesh mesh, Material[] mats, Transform tf)[] mmts)
        {
            var qPartIdPerMesh =
                from mmt in mmts
                    //.Do(x => Debug.Log($"part id is {x.tf.getInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.getInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                    //.Do(x => Debug.Log($"part id is {x.tf.findInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.findInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                select mmt.tf.getInParent<IStructurePart>()?.partId ?? -1
                //select mmt.tf.gameObject.GetComponentInParent<StructurePartAuthoring>()?.PartId ?? -1
                ;
            parameters.partIdPerMesh = qPartIdPerMesh.ToArray();
        }

    }
}