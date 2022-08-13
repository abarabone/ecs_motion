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

namespace DotsLite.Geometry.inner
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Misc;


    static partial class ConvertVertexUtility
    {


        static public IEnumerable<Vector3> QueryConvertPositionsWithBone
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in (srcmeshes, p.mtInvsPerMesh, p.boneWeightsPerMesh, p.mtPerMesh).Zip()
            let mesh = permesh.src0.MeshData
            let mtInvs = permesh.src1// Ç±ÇÍÇ¢Ç¢ÇÃÇ©ÅH
            let weis = permesh.src2
            //let mt = permesh.src3 * p.mtBaseInv//p.mtBaseInv
            let mt = math.mul(p.mtBaseInv, permesh.src3)//p.mtBaseInv
            from x in (mesh.QueryMeshVertices<Vector3>((md, arr) => md.GetVertices(arr), VertexAttribute.Position), weis).Zip()
            let vtx = x.src0
            let wei = x.src1
            select (Vector3)math.transform(math.mul(mtInvs[wei.boneIndex0], mt), vtx)
            ;




        static public IEnumerable<uint> QueryConvertBoneIndices
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in p.boneWeightsPerMesh.WithIndex()
            from w in permesh.src
            select (uint)(
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex0] <<  0 & 0xff |
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex1] <<  8 & 0xff |
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex2] << 16 & 0xff |
                p.srcBoneIndexToDstBoneIndex[permesh.i, w.boneIndex3] << 24 & 0xff
            );




        static public IEnumerable<Vector4> QueryConvertBoneWeights
            (this IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
        =>
            from permesh in p.boneWeightsPerMesh
            from w in permesh
            select new Vector4(w.weight0, w.weight1, w.weight2, w.weight3)
            ;


    }



}
