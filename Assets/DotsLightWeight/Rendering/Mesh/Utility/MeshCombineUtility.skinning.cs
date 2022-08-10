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

        public static void calculateBoneParameters(
            this AdditionalParameters parameters, (Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform[] tfBones)
        {
            var qMeshes = mmts
                .Select(x => x.mesh);

            var qBoneWeights =
                from mesh in qMeshes
                select mesh.boneWeights
                ;
            var qMtInvs =
                from mesh in qMeshes
                select mesh.bindposes
                ;
            var qSrcBones = mmts
                .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.WrapEnumerable().ToArray());
            ;
            //qSrcBones.SelectMany().ForEach(x => Debug.Log(x.name));
            parameters.boneWeightsPerMesh = qBoneWeights.ToArray();
            parameters.mtInvsPerMesh = qMtInvs.ToArray();
            parameters.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
        }

    }
}